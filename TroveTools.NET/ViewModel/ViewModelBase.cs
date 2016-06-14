using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Framework;

namespace TroveTools.NET.ViewModel
{
    public abstract class ViewModelBase : DynamicObject, INotifyPropertyChanged, IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructor
        public ViewModelBase() { }
        #endregion // Constructor

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have a public property with the specified
        /// name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real, public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used when an invalid
        /// property name is passed to the VerifyPropertyName method. The default value is false,
        /// but subclasses used by unit tests might override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        /// <summary>
        /// Returns the user-friendly name of this object. Child classes can set this property to a
        /// new value, or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName { get; protected set; }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members

        #region IDisposable Members
        /// <summary>
        /// Invoked when this object is being removed from the application and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Child classes can override this method to perform clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose() { }
        #endregion // IDisposable Members
    }

    public abstract class ViewModelBase<TModel> : ViewModelBase
    {
        #region Constructor
        public ViewModelBase(TModel dataObject)
        {
            DataObject = dataObject;
        }
        #endregion // Constructor

        internal TModel DataObject { get; }

        #region DynamicObject overrides for automatic proxy to model properties
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            PropertyInfo property = DataObject?.GetType().GetProperty(binder.Name);
            if (property == null || property.CanRead == false)
            {
                result = null;
                return false;
            }
            result = property.GetValue(DataObject, null);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            PropertyInfo property = DataObject?.GetType().GetProperty(binder.Name);
            if (property == null || property.CanWrite == false) return false;

            property.SetValue(DataObject, value, null);
            RaisePropertyChanged(binder.Name);

            var affectsProps = property.GetCustomAttributes(typeof(AffectsPropertyAttribute), true);
            foreach (AffectsPropertyAttribute propertyAttribute in affectsProps)
                RaisePropertyChanged(propertyAttribute.PropertyName);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type dataObjectType = DataObject?.GetType();
            Type[] argsTypes = args.Select(a => a?.GetType()).ToArray();
            MethodInfo method = dataObjectType?.GetMethod(binder.Name, argsTypes);
            if (dataObjectType == null || method == null)
            {
                result = null;
                return false;
            }
            result = dataObjectType.InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, DataObject, args);

            var affectsProps = method.GetCustomAttributes(typeof(AffectsPropertyAttribute), true);
            foreach (AffectsPropertyAttribute propertyAttribute in affectsProps)
                RaisePropertyChanged(propertyAttribute.PropertyName);

            return true;
        }
        #endregion

#if DEBUG
        #region Debugging Aides
        /// <summary>
        /// Warns the developer if this object does not have a public property with the specified
        /// name. This method does not exist in a Release build.
        /// </summary>
        [DebuggerStepThrough]
        public override void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real, public, instance property on this object or the DataObject
            if (TypeDescriptor.GetProperties(this)[propertyName] == null && TypeDescriptor.GetProperties(DataObject)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        #endregion
#endif
    }
}
