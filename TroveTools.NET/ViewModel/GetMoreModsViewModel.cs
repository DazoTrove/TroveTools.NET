using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TroveTools.NET.Framework;
using TroveTools.NET.Model;
using TroveTools.NET.Properties;

namespace TroveTools.NET.ViewModel
{
    class GetMoreModsViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DelegateCommand _refreshCommand, _clearSearchCommand, _launchTrovesaurusCommand;
        private DelegateCommand<string> _sortCommand;
        private CollectionViewSource _modsView = new CollectionViewSource(), _typesView = new CollectionViewSource(), _subTypesView = new CollectionViewSource();

        #region Constructor
        public GetMoreModsViewModel()
        {
            DisplayName = Strings.GetMoreModsViewModel_DisplayName;
        }
        #endregion // Constructor

        public void LoadData()
        {
            // Load mods data from API
            try
            {
                // Load mods from model and create view model objects
                IsLoading = true;
                RefreshMods();

                // Build Type and SubTypes lists using LINQ groupby queries
                IsLoading = true;
                var textInfo = CultureInfo.CurrentCulture.TextInfo;

                Types = new ObservableCollection<string>(
                    new string[] { Strings.GetMoreModsViewModel_AllTypes }.Union(from m in TrovesaurusApi.ModList
                                                                                 group m by m.Type into g
                                                                                 orderby g.First().Type
                                                                                 select textInfo.ToTitleCase(g.First().Type.ToLower())));

                SubTypes = new ObservableCollection<string>(
                    new string[] { Strings.GetMoreModsViewModel_AllSubTypes }.Union(from m in TrovesaurusApi.ModList
                                                                                    where !string.IsNullOrEmpty(m.SubType)
                                                                                    group m by m.SubType into g
                                                                                    orderby g.First().SubType
                                                                                    select textInfo.ToTitleCase(g.First().SubType.ToLower())));

                // Set Collection View Source for collections for current item tracking, sorting, and filtering
                _modsView.Source = TrovesaurusMods;
                _typesView.Source = Types;
                _subTypesView.Source = SubTypes;

                // Setup current item changing events
                TypesView.CurrentChanged += (s, e) => TypeFilter = TypesView.CurrentItem as string;
                SubTypesView.CurrentChanged += (s, e) => SubTypeFilter = SubTypesView.CurrentItem as string;

                // Setup filter function
                TrovesaurusModsView.Filter = ModFilter;

                // Set default sort to total downloads
                SortModList("TotalDownloads");
            }
            catch (Exception ex)
            {
                log.Error("Error loading mods from Trovesaurus API", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Properties
        public ObservableCollection<TroveModViewModel> TrovesaurusMods { get; } = new ObservableCollection<TroveModViewModel>();

        public ObservableCollection<string> Types { get; set; }

        public ObservableCollection<string> SubTypes { get; set; }

        public ICollectionView TrovesaurusModsView
        {
            get { return _modsView.View; }
        }

        public ICollectionView TypesView
        {
            get { return _typesView.View; }
        }

        public ICollectionView SubTypesView
        {
            get { return _subTypesView.View; }
        }

        private bool _IsLoading = false;
        public bool IsLoading
        {
            get { return _IsLoading; }
            set
            {
                _IsLoading = value;
                RaisePropertyChanged("IsLoading");
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }

        private string _SearchFilter = string.Empty;
        public string SearchFilter
        {
            get { return _SearchFilter; }
            set
            {
                _SearchFilter = value;
                RaisePropertyChanged("SearchFilter");
                TrovesaurusModsView.Refresh();
            }
        }

        private string _TypeFilter = Strings.GetMoreModsViewModel_AllTypes;
        public string TypeFilter
        {
            get { return _TypeFilter; }
            set
            {
                _TypeFilter = value;
                RaisePropertyChanged("TypeFilter");
                TrovesaurusModsView.Refresh();
            }
        }

        private string _SubTypeFilter = Strings.GetMoreModsViewModel_AllSubTypes;
        public string SubTypeFilter
        {
            get { return _SubTypeFilter; }
            set
            {
                log.DebugFormat("Set SubTypeFilter to [{0}]", value);

                _SubTypeFilter = value;
                RaisePropertyChanged("SubTypeFilter");
                TrovesaurusModsView.Refresh();
            }
        }

        private ListSortDirection _SortDirection;
        public ListSortDirection SortDirection
        {
            get { return _SortDirection; }
            set
            {
                _SortDirection = value;
                RaisePropertyChanged("SortDirection");
            }
        }

        private string _SortBy;
        public string SortBy
        {
            get { return _SortBy; }
            set
            {
                _SortBy = value;
                RaisePropertyChanged("SortBy");
            }
        }
        #endregion

        #region Commands
        public DelegateCommand LaunchTrovesaurusCommand
        {
            get
            {
                if (_launchTrovesaurusCommand == null) _launchTrovesaurusCommand = new DelegateCommand(LaunchTrovesaurus);
                return _launchTrovesaurusCommand;
            }
        }

        public DelegateCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null) _refreshCommand = new DelegateCommand(RefreshMods, p => !IsLoading);
                return _refreshCommand;
            }
        }

        public DelegateCommand ClearSearchCommand
        {
            get
            {
                if (_clearSearchCommand == null) _clearSearchCommand = new DelegateCommand(ClearSearch);
                return _clearSearchCommand;
            }
        }

        public DelegateCommand<string> SortCommand
        {
            get
            {
                if (_sortCommand == null) _sortCommand = new DelegateCommand<string>(SortModList);
                return _sortCommand;
            }
        }
        #endregion

        #region Private methods
        private void ClearSearch(object param = null)
        {
            SearchFilter = string.Empty;
        }

        private void LaunchTrovesaurus(object param = null)
        {
            try
            {
                TrovesaurusApi.LaunchTrovesaurus();
            }
            catch (Exception ex)
            {
                log.Error("Error launching Trovesaurus", ex);
            }
        }

        private void RefreshMods(object param = null)
        {
            try
            {
                IsLoading = true;
                log.Info("Loading mods from Trovesaurus API");
                TrovesaurusApi.RefreshModList();
                TrovesaurusMods.Clear();
                foreach (TroveMod mod in TrovesaurusApi.ModList)
                {
                    TrovesaurusMods.Add(new TroveModViewModel(mod));
                }
                log.Info("Loading mod list complete");
            }
            catch (Exception ex)
            {
                log.Error("Error refreshing mods", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ModFilter(object item)
        {
            TroveModViewModel mod = item as TroveModViewModel;
            var ic = StringComparison.OrdinalIgnoreCase;

            bool search = true, types = true, subtypes = true;
            try
            {
                search = string.IsNullOrEmpty(SearchFilter) || mod.DataObject.Name?.IndexOf(SearchFilter, ic) >= 0 || mod.DataObject.Author?.IndexOf(SearchFilter, ic) >= 0 ||
                    mod.DataObject.Type?.IndexOf(SearchFilter, ic) >= 0 || mod.DataObject.SubType?.IndexOf(SearchFilter, ic) >= 0;

                types = string.IsNullOrEmpty(TypeFilter) || TypeFilter.Equals(Strings.GetMoreModsViewModel_AllTypes, ic) ||
                    mod.DataObject.Type == null || mod.DataObject.Type.Equals(TypeFilter, ic);

                subtypes = string.IsNullOrEmpty(SubTypeFilter) || SubTypeFilter.Equals(Strings.GetMoreModsViewModel_AllSubTypes, ic) ||
                    mod.DataObject.SubType == null || mod.DataObject.SubType.Equals(SubTypeFilter, ic);
            }
            catch (Exception ex)
            {
                log.Error("Mod filter error", ex);
                if (log.IsDebugEnabled) log.DebugFormat("Mod Name: [{0}], Author: [{1}], Type: [{2}], SubType: [{3}], SearchFilter: [{4}], TypeFilter: [{5}], SubTypeFilter: [{6}]",
                    mod?.DataObject?.Name, mod?.DataObject?.Author, mod?.DataObject?.Type, mod?.DataObject?.SubType, SearchFilter, TypeFilter, SubTypeFilter);
            }
            return search && types && subtypes;
        }

        private void SortModList(string column)
        {
            try
            {
                // Toggle sort direction if previously sorted by this column
                if (SortBy == column)
                    SortDirection = SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                else
                {
                    // Set sort column and initial sort direction
                    SortBy = column;
                    SortDirection = column == "TotalDownloads" || column == "Views" || column == "LastUpdated" ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                log.InfoFormat("Sorting by {0}, direction: {1}", SortBy, SortDirection);

                TrovesaurusModsView.SortDescriptions.Clear();
                TrovesaurusModsView.SortDescriptions.Add(new SortDescription(SortBy, SortDirection));
            }
            catch (Exception ex)
            {
                log.Error("Error sorting mod list", ex);
            }
        }
        #endregion
    }
}
