using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MapWinGIS;
using ResTB.Calculation;
using ResTB.DB;
using ResTB.DB.Models;
using ResTB.GUI.Helpers;
using ResTB.GUI.Helpers.Converter;
using ResTB.GUI.Helpers.Geocode;
using ResTB.GUI.Helpers.Messages;
using ResTB.GUI.Model;
using ResTB.GUI.Model.Layers;
using ResTB.GUI.View;
using ResTB.Map;
using ResTB.Map.Events;
using ResTB.Map.Layer;
using ResTB.Map.Tools;
using ResTB.Translation.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace ResTB.GUI.ViewModel
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum myTkTileProvider
    {
        [Description("None")]
        ProviderNone = -1,
        [Description("Open Street Map")]
        OpenStreetMap = 0,
        //OpenCycleMap = 1,
        //OpenTransportMap = 2,
        [Description("Bing Maps")]
        BingMaps = 3,
        [Description("Bing Satellite")]
        BingSatellite = 4,
        [Description("Bing Hybrid")]
        BingHybrid = 5,
        [Description("Open Humanitarian Map")]
        OpenHumanitarianMap = 22,
        //ProviderCustom = 1024,
        [Description("Google Satellite")]
        GoogleSatellite = 1025

    }

    public class MainViewModel : ViewModelBase
    {
        public bool IsBusy { get; set; }
        public bool IsBackgroundBusy { get; set; }
        public System.Windows.Input.Cursor Cursor
        {
            get
            {
                if (IsBusy)
                    return System.Windows.Input.Cursors.Wait;
                else
                    return null;
            }
        }

        public ResTBPostGISType SelectedTabType { get; private set; }

        public bool UseOnlineDB { get; set; }
        public bool HasDBConnection { get; set; }
        public bool HasInternetConnection { get; set; }
        public string WindowTitle => $"{Resources.App_Name}{(Project != null ? $": {Project?.Name}" : "")}";

        public myTkTileProvider CurrentTileProvider { get; set; } = myTkTileProvider.GoogleSatellite;
        //public IEnumerable<myTkTileProvider> TileProviderValues
        //{
        //    get
        //    {
        //        return Enum.GetValues(typeof(myTkTileProvider))
        //            .Cast<myTkTileProvider>()
        //            //.Where(t => !t.ToString().ToLower().Contains("here"))
        //            //.Where(t => !t.ToString().ToLower().Contains("opencycle"))
        //            //.Where(t => !t.ToString().ToLower().Contains("opentransport"))
        //            //.Where(t => !t.ToString().ToLower().Contains("rosreestr"))
        //            //.Where(t => !t.ToString().ToLower().Contains("mapquest"))
        //            .Where(t => !t.ToString().ToLower().Contains("custom"))
        //            ;
        //    }
        //}

        public tkKnownExtents CurrentExtend { get; set; } = tkKnownExtents.keHonduras;
        public IEnumerable<tkKnownExtents> ExtendValues
        {
            get
            {
                var enums = Enum.GetValues(typeof(tkKnownExtents))
                    .Cast<tkKnownExtents>()
                    ;
                return enums;
            }
        }

        public bool ShowToolColumn { get; set; } = true;

        public Project Project { get; set; }
        public Project NewProject { get; set; }
        private Project selectedProject;
        public Project SelectedProject
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                if (value != null)
                    UpdateCommandsCanExecute();
            }
        }

        public ObservableCollection<Project> AllProjects { get; private set; } = new ObservableCollection<Project>();

        public ObservableCollection<NatHazard> NatHazards { get; private set; } = new ObservableCollection<NatHazard>();
        public NatHazard SelectedNatHazard { get; set; }

        public List<HazardIndex> HazardIndexes { get; private set; } = new List<HazardIndex>() {
            new HazardIndex(1),
            new HazardIndex(2),
            new HazardIndex(3),
            new HazardIndex(4),
            new HazardIndex(5),
            new HazardIndex(6),
            new HazardIndex(7),
            new HazardIndex(8),
            new HazardIndex(9),
        };
        public HazardIndex SelectedHazardIndex { get; set; }
        public HazardMap SelectedHazardMap { get; set; }

        public ObservableCollection<ObjectClass> ObjectClasses { get; set; }
        public ObservableCollection<Objectparameter> ObjectParameters { get; set; }
        public ObservableCollection<Objectparameter> FilteredObjectParameters
        {
            get
            {
                if (ObjectParameters != null)
                {
                    var filterdList = ObjectParameters.Where(o => o.ObjectClass == SelectedObjectClass).OrderBy(o => o.Name);
                    SelectedObjectParameter = filterdList.FirstOrDefault();
                    return new ObservableCollection<Objectparameter>(filterdList);
                }
                else
                {
                    return new ObservableCollection<Objectparameter>();
                }
            }
        }
        public ObjectClass SelectedObjectClass { get; set; }
        public Objectparameter SelectedObjectParameter { get; set; }
        public MappedObject SelectedMappedObject { get; set; }
        public Objectparameter SelectedMergedObjectParameter { get; set; }

        public PropertyDefinitionCollection SelectedMergedPropertyDefinitions { get; set; }

        public ObservableCollection<LayersModel> MapLayers { get; set; } = new ObservableCollection<LayersModel>();
        private LayersModel selectedLayer;
        public LayersModel SelectedLayer
        {
            get => selectedLayer;
            set
            {
                selectedLayer = value;
                UpdateCommandsCanExecute();
            }
        }

        public Geocoder GeoCoder { get; set; }
        public Place SelectedPlace { get; set; }
        public Place GoToCoordinates { get; set; } = new Place() { name = "goto" };
        public ObservableCollection<Place> Places { get; set; } = new ObservableCollection<Place>();
        public string PlaceFilterString { get; set; } = String.Empty;
        public ObservableCollection<Place> FilteredPlaces =>
            new ObservableCollection<Place>(Places.Where(p => p.name.ToLower().Contains(PlaceFilterString.Trim().ToLower())));

        public bool HasPerimeter
        {
            get
            {
                if ((MapLayers.Count > 0) && (MapLayers.First().Children != null))
                {
                    List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());
                    return allSubLayers?.Any(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.Perimeter) ?? false;
                }
                else return false;
            }
        }

        public bool HasPerimeterShape
        {
            get
            {
                if ((MapLayers.Count > 0) && (MapLayers.First().Children != null))
                {
                    List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());

                    LayersModel layerModel = allSubLayers?
                        .FirstOrDefault(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.Perimeter);

                    if (layerModel != null && layerModel.Layer != null)
                    {
                        return layerModel.Layer.ShapeCount > 0;
                    }
                    else return false;
                }
                else return false;
            }
        }

        public bool HasHazardMapBeforeShape
        {
            get
            {
                if ((MapLayers.Count > 0) && (MapLayers.First().Children != null))
                {
                    List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());

                    LayersModel layerModel = allSubLayers?
                        .FirstOrDefault(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.HazardMapBefore);

                    if (layerModel != null && layerModel.Layer != null)
                    {
                        return layerModel.Layer.ShapeCount > 0;
                    }
                    else return false;
                }
                else return false;
            }
        }

        public bool HasDamagePotentialShape
        {
            get
            {
                if ((MapLayers.Count > 0) && (MapLayers.First().Children != null))
                {
                    List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());

                    LayersModel layerModel = allSubLayers?
                        .FirstOrDefault(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.DamagePotential);

                    if (layerModel != null && layerModel.Layer != null)
                    {
                        return layerModel.Layer.ShapeCount > 0;
                    }
                    else return false;
                }
                else return false;
            }
        }

        public MapControl MapControl { get; internal set; }

        public bool IsEditingMap { get; set; } = false;
        public bool IsAddingShape { get; set; } = false;
        public bool IsCopyingResilience { get; set; } = false;

        public string StatusBarMainString { get; set; }
        public bool StatusBarProgressBarVisible { get; set; } = false;
        public int StatusBarProgressBarPercent { get; set; } = 0;
        public string StatusBarProgressBarMessageString { get; set; } = "";
        public bool StatusBarIndeterminate { get; set; } = false;

        public NewProjectWindow NewProjectWindow { get; private set; }
        public OpenProjectWindow OpenProjectWindow { get; private set; }

        //private readonly ISampleService sampleService;
        private int SelectedShapeIndex { get; set; } = -1;

        public MainViewModel()
        {
            if (ConfigurationManager.AppSettings["UseOfflineDB"] == "true")
            {
                DB.DBUtils.Instance.StartLocalDB();
                UseOnlineDB = false;
            }
            else
            {
                UseOnlineDB = true;
            }


            UpdateDBConnection();
            UpdateInternetConnection();


            // Layer sample
            this.MapLayers = new ObservableCollection<LayersModel>();
            //this.Layers = LayersModel.CreateFoos();


            if (HasDBConnection)
            {
                LoadNatHazards();
                LoadObjectParameters();
            }

            GeoCoder = new Geocoder("HN");  //HN oder CH
            if (GeoCoder.Places != null)
                Places = new ObservableCollection<Place>(GeoCoder.Places);

            Messenger.Default.Register<MapMessage>(
                    this,
                    message =>
                    {
                        //MessageBoxMessage.Send("MapMessage Received", $"{message.MessageType}", true);
                        string status = $"MAP {message.MessageType}: ";
                        switch (message.MessageType)
                        {
                            case MapMessageType.Default:
                                break;
                            case MapMessageType.Message:
                                status += message.Message;
                                break;
                            case MapMessageType.KnownExtent:
                                status += message.KnownExtent;
                                break;
                            case MapMessageType.TileProvider:
                                status += (myTkTileProvider)message.TileProviderId;
                                break;
                            case MapMessageType.CursorMode:
                                status += message.CursorMode;
                                break;
                            case MapMessageType.Initialized:
                                status += message.Boolean;
                                MapControl.Tools.MapControl_Error += MapControl_Error;
                                MapControl.Tools.MapControl_LayerChange += MapControl_LayerChange;
                                MapControl.Tools.MapControl_EditingStateChange += MapControl_EditingStateChange;
                                MapControl.Tools.MapControl_Clicked += MapControl_Clicked;
                                MapControl.Tools.MapControl_BusyStateChange += MapControl_BusyStateChange;
                                MapControl.Tools.MapControl_SelectingStateChange += MapControl_SelectingStateChange;

                                this.SetupMap();
                                break;
                            default:
                                break;
                        }

                        if (Project != null)
                        {
#if DEBUG
                            StatusBarMainString = status;
#endif
                        }
                    });

        }

        private void UpdateDBConnection()
        {
            bool dbExists = false;

            using (ResTBContext db = new ResTBContext())
            {
                dbExists = db.Database.Exists();
            }

            HasDBConnection = dbExists;
        }

        private void UpdateInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    HasInternetConnection = true;
            }
            catch
            {
                HasInternetConnection = false;
            }
        }

        private async void LoadNatHazards()
        {
            List<NatHazard> hazards = new List<NatHazard>();

            using (ResTBContext db = new DB.ResTBContext())
            {
                hazards = await db.NatHazards.AsNoTracking().Where(n => n.ID > 2).OrderBy(h => h.Name).ToListAsync(); //id=2 is used for resilience weights default!
            }

            NatHazards = new ObservableCollection<NatHazard>(hazards);
            SelectedNatHazard = NatHazards.FirstOrDefault();

            SelectedHazardIndex = HazardIndexes.FirstOrDefault();
        }

        private async void LoadObjectParameters()
        {
            List<Objectparameter> objectParameters = new List<Objectparameter>();

            using (ResTBContext db = new DB.ResTBContext())
            {
                objectParameters = await db.Objektparameter
                    .Include(op => op.ObjectClass)
                    .Where(o => o.IsStandard)
                    .OrderBy(o => o.Name)
                    .ToListAsync();

                ObjectParameters = new ObservableCollection<Objectparameter>(objectParameters);

                var objectClassList = ObjectParameters.Select(o => o.ObjectClass).OrderBy(c => c.ID).Distinct().ToList();
                ObjectClasses = new ObservableCollection<ObjectClass>(objectClassList);
            }

            SelectedObjectClass = ObjectClasses.FirstOrDefault();
            SelectedObjectParameter = ObjectParameters.Where(o => o.ObjectClass == SelectedObjectClass).FirstOrDefault();
        }

        private void SetupMap()
        {
            MapControl.Tools.RemoveAllLayers();

            MapControl.Tools.AxMap.CursorMode = tkCursorMode.cmPan;
            MapControl.Tools.AxMap.GrabProjectionFromData = true;
            MapControl.Tools.AxMap.AnimationOnZooming = tkCustomState.csFalse;
            MapControl.Tools.AxMap.InertiaOnPanning = tkCustomState.csFalse;
            MapControl.Tools.AxMap.MapResizeBehavior = tkResizeBehavior.rbKeepScale;
            MapControl.Tools.AxMap.ZoomBarMaxZoom = 19;                                 //20 -> bing maps not displayed

            MapControl.Tools.AxMap.Projection = tkMapProjection.PROJECTION_GOOGLE_MERCATOR;

            //Google Satellite Tile Provide
            TileProviders providers = MapControl.Tools.AxMap.Tiles.Providers;
            int providerId = (int)myTkTileProvider.GoogleSatellite;
            providers.Add(providerId, "Google",
            "http://mt.google.com/vt/lyrs=s&x={x}&y={y}&z={zoom}",
            tkTileProjection.SphericalMercator, 0, 20, "Google");
            //MapControl.Tools.AxMap.Tiles.ProviderId = providerId;

            MapControl.Tools.AxMap.Tiles.ProviderId = (int)CurrentTileProvider;
            MapControl.Tools.AxMap.KnownExtents = CurrentExtend;

            //Caching
            MapControl.Tools.AxMap.Tiles.set_DoCaching(tkCacheType.RAM, true);  // is on by default
            MapControl.Tools.AxMap.Tiles.set_DoCaching(tkCacheType.Disk, true); // if off by default

            string localData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!System.IO.Directory.Exists(localData + "\\ResTBDesktop")) System.IO.Directory.CreateDirectory(localData + "\\ResTBDesktop");
            MapControl.Tools.AxMap.Tiles.DiskCacheFilename = localData + "\\ResTBDesktop\\tiles_cache.db3";




        }

        private void UpdateMapLayers()
        {
            // Hierarchical Layers

            LayersModel tempSelectedLayer = SelectedLayer;

            LayersModel newLayersModel = LayersModel.CreateLayersModel(MapControl.Tools.Layers, tempSelectedLayer, MapLayers);
            MapLayers.Clear();
            newLayersModel.Children?.ToList().ForEach(MapLayers.Add);
            //MapLayers.Add(LayersModel.CreateLayersModel(MapControl.Tools.Layers, tempSelectedLayer));
            SelectedLayer = tempSelectedLayer;

            SelectLayerByActiveTab();
        }

        /// <summary>
        /// not needed anymore
        /// issue due to using GalaSoft.MvvmLight.Command instead of using GalaSoft.MvvmLight.CommandWpf
        /// </summary>
        private void UpdateCommandsCanExecute()
        {

            //            Type myType = this.GetType();
            //            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            //            foreach (PropertyInfo prop in props)
            //            {
            //                if (prop.PropertyType.ToString().StartsWith(typeof(RelayCommand).ToString()))
            //                {

            //                    try
            //                    {
            //                        dynamic dynamicProp = prop.GetValue(this, null);
            //                        dynamicProp.RaiseCanExecuteChanged();

            //                        //var cmd = (RelayCommand)prop.GetValue(this, null);
            //                        //cmd.RaiseCanExecuteChanged();
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        //do nothing                        
            //#if DEBUG
            //                        throw ex;
            //#endif
            //                    }

            //                }
            //            }
        }

        // COMMANDS

        private RelayCommand<Window> _closeCommand;
        public RelayCommand<Window> CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new RelayCommand<Window>(
                    window =>
                    {
                        window?.Close();
                        UpdateCommandsCanExecute();
                    }));
            }
        }

        private RelayCommand _closingAppCommand;
        public RelayCommand ClosingAppCommand
        {
            get
            {
                return _closingAppCommand
                    ?? (_closingAppCommand = new RelayCommand(
                    () =>
                    {
                        if (ConfigurationManager.AppSettings["UseOfflineDB"] == "true")
                            DBUtils.Instance.StopLocalDB();

                    }));
            }
        }

        private RelayCommand<string> _helpCommand;
        public RelayCommand<string> HelpCommand
        {
            get
            {
                return _helpCommand
                    ?? (_helpCommand = new RelayCommand<string>(
                    helptopic =>
                    {
                        if (!string.IsNullOrWhiteSpace(helptopic))
                            Helpers.HelpSystem.HelpProvider.ShowHelpTopic(helptopic);
                        else
                            Helpers.HelpSystem.HelpProvider.ShowHelpTableOfContents();
                    }));
            }
        }
        public TabItem TabControlSelectedItem { get; set; }

        private int tabControlSelectedIndex;
        public int TabControlSelectedIndex
        {
            get => tabControlSelectedIndex;
            set
            {
                tabControlSelectedIndex = value; // on set: TabSwitchCommand

                switch (tabControlSelectedIndex)    //TODO: improve: switch by SelectedItem.Name instead of magic integer
                {
                    case 0:
                        TabSwitchCommand.Execute(ResTBPostGISType.Perimeter);
                        break;
                    case 1:
                        TabSwitchCommand.Execute(ResTBPostGISType.HazardMapBefore);
                        break;
                    case 2:
                        TabSwitchCommand.Execute(ResTBPostGISType.DamagePotential);
                        break;
                    case 3:
                        TabSwitchCommand.Execute(ResTBPostGISType.ResilienceBefore);
                        break;
                    case 4:
                        TabSwitchCommand.Execute(ResTBPostGISType.MitigationMeasure);
                        break;
                    case 5:
                        TabSwitchCommand.Execute(ResTBPostGISType.HazardMapAfter);
                        break;
                    case 6:
                        TabSwitchCommand.Execute(ResTBPostGISType.ResilienceAfter);
                        break;
                    case 7:
                        TabSwitchCommand.Execute(ResTBPostGISType.RiskMap);
                        break;
                    default:
                        break;
                }
            }
        }

        private RelayCommand<ResTBPostGISType> _tabSwitchCommand;
        public RelayCommand<ResTBPostGISType> TabSwitchCommand
        {
            get
            {
                return _tabSwitchCommand
                    ?? (_tabSwitchCommand = new RelayCommand<ResTBPostGISType>(
                    layerType =>
                    {
                        SelectedTabType = layerType;

                        StopSelectionMapCommand.Execute(null);
                        SelectedShapeIndex = -1;

                        if (layerType == ResTBPostGISType.HazardMapAfter || layerType == ResTBPostGISType.HazardMapBefore)
                        {
                            SelectedHazardMap = null;
                        }
                        else if (layerType == ResTBPostGISType.ResilienceAfter || layerType == ResTBPostGISType.ResilienceBefore)
                        {
                            if (SelectedMappedObject != null && SelectedMergedObjectParameter != null)
                            {
                                //deselect mapped object if no resilience factors available
                                if (!SelectedMergedObjectParameter.ResilienceValuesMerged.Any())
                                {
                                    SelectedMappedObject = null;
                                    SelectedMergedObjectParameter = null;
                                }
                            }
                        }

                        //MessageBoxMessage.Send("INFO", $"clicked on {layertype}");

                        SelectLayerByActiveTab();

                        //Default: layer selection activated
                        switch (layerType)
                        {
                            case ResTBPostGISType.Perimeter:
                                break;
                            case ResTBPostGISType.HazardMapBefore:
                                SelectHazardMapCommand.Execute(true);
                                break;
                            case ResTBPostGISType.DamagePotential:
                                SelectDamagePotentialCommand.Execute(null);
                                break;
                            case ResTBPostGISType.ResilienceBefore:
                                SelectResilienceCommand.Execute(true);
                                break;
                            case ResTBPostGISType.MitigationMeasure:
                                break;
                            case ResTBPostGISType.HazardMapAfter:
                                SelectHazardMapCommand.Execute(false);
                                break;
                            case ResTBPostGISType.ResilienceAfter:
                                SelectResilienceCommand.Execute(false);
                                break;
                            case ResTBPostGISType.RiskMap:
                                break;
                            default:
                                break;
                        }

                    }));
            }
        }

        private void SelectLayerByActiveTab()
        {
            if (MapLayers.Any())
            {
                var layerList = MapLayers.FirstOrDefault()?.getAllChildren(new List<LayersModel>());

                var layerModels = layerList?.Where(l => l.Layer is ResTBPostGISLayer);

                if (layerModels != null && layerModels.Any())
                {
                    foreach (var layerModel in layerModels.Where(l => l.Layer != null))
                    {
                        ResTBPostGISLayer resTbLayer = ((ResTBPostGISLayer)layerModel.Layer);
                        if (resTbLayer.ResTBPostGISType == SelectedTabType)
                        {
                            layerModel.IsChecked = true;
                        }
                        else
                        {
                            layerModel.IsChecked = false;
                        }
                    }
                }

                ChangeLayerVisibilityCommand.Execute(null);
            }
        }

        #region ResultCommands

        public bool IsCalculating { get; set; } = false;

        private RelayCommand<bool> _runKernelCommand;
        public RelayCommand<bool> RunKernelCommand
        {
            get
            {
                return _runKernelCommand
                    ?? (_runKernelCommand = new RelayCommand<bool>(
                    async showDetails =>
                    {
                        bool onlySummary = false;

                        IsCalculating = true;
                        UpdateCommandsCanExecute();
                        //Messenger.Default.Send(new HtmlMessage() { HtmlString = String.Empty });

                        IsBackgroundBusy = true;
                        StatusBarMainString = string.Empty;
                        StatusBarProgressBarVisible = true;
                        StatusBarProgressBarMessageString = Resources.Result_CalcStarted;
                        StatusBarIndeterminate = true;

                        var calc = new Calc(Project.Id);

                        if (!onlySummary)
                        {
                            calc.CreateIntensityMaps();                             //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                            StatusBarProgressBarMessageString = Resources.Result_IntensMapCreated;
                        }

                        string returnvalue = "";
                        returnvalue = await calc.RunCalculationAsync(onlySummary, showDetails, UseOnlineDB);  //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                        if (returnvalue.ToLower().Contains("error"))
                        {
                            MessageBoxMessage.Send("Error", $"{returnvalue}", true);
                            StatusBarMainString = $"Error";
                        }
                        else
                        {
#if DEBUG
                            MessageBoxMessage.Send("Calculation finished", $"{returnvalue}", false);
#endif
                            StatusBarMainString = Resources.Result_CalcFinished;

                            /////////////////////////////////////////
                            string localData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            if (!System.IO.Directory.Exists(localData + "\\ResTBDesktop"))
                                System.IO.Directory.CreateDirectory(localData + "\\ResTBDesktop");
                            string htmlPath = localData + "\\ResTBDesktop\\result.html";


                            System.Diagnostics.Process.Start(htmlPath);         //<<<<<<< Start in default browser

                            //Messenger.Default.Send(new HtmlMessage() { HtmlString = File.ReadAllText(htmlPath) });
                            //Messenger.Default.Send(new HtmlMessage() { Url = htmlPath });

                            /////////////////////////////////////////

                            //LOAD LAYER RESULT <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                            MapControl.Tools.AddProjectLayer(Project.Id, ResTBPostGISType.RiskMap);
                            MapControl.Tools.Redraw(true);
                            UpdateMapLayers();

                        }
                        StatusBarIndeterminate = false;
                        IsBackgroundBusy = false;
                        StatusBarProgressBarVisible = false;

                        IsCalculating = false;
                        UpdateCommandsCanExecute();
                    },
                    showDetails => !IsCalculating && !IsBusy && !IsBackgroundBusy && HasPerimeterShape && HasHazardMapBeforeShape && HasDamagePotentialShape
                    ));
            }
        }

        #endregion

        #region MapCommands

        private RelayCommand<myTkTileProvider> _mapProviderCommand;
        public RelayCommand<myTkTileProvider> MapProviderCommand
        {
            get
            {
                return _mapProviderCommand
                    ?? (_mapProviderCommand = new RelayCommand<myTkTileProvider>(
                    obj =>
                    {
                        var message = new MapMessage()
                        {
                            MessageType = MapMessageType.TileProvider,
                            TileProviderId = (int)obj
                        };
                        Messenger.Default.Send(message);
                    },
                    obj => true));
            }
        }

        private RelayCommand<tkKnownExtents> _mapExtendCommand;
        public RelayCommand<tkKnownExtents> MapExtendCommand
        {
            get
            {
                return _mapExtendCommand
                    ?? (_mapExtendCommand = new RelayCommand<tkKnownExtents>(
                    obj =>
                    {
                        var message = new MapMessage()
                        {
                            MessageType = MapMessageType.KnownExtent,
                            KnownExtent = obj
                        };
                        Messenger.Default.Send(message);
                    },
                    obj => true)
                    );
            }
        }

        private RelayCommand<tkCursorMode> _mapCursorModeCommand;
        public RelayCommand<tkCursorMode> MapCursorModeCommand
        {
            get
            {
                return _mapCursorModeCommand
                    ?? (_mapCursorModeCommand = new RelayCommand<tkCursorMode>(
                    obj =>
                    {
                        var message = new MapMessage()
                        {
                            MessageType = MapMessageType.CursorMode,
                            CursorMode = obj
                        };
                        Messenger.Default.Send(message);
                    },
                    obj => true));
            }
        }

        private RelayCommand<string> _messageBoxCommand;
        public RelayCommand<string> MessageBoxCommand
        {
            get
            {
                return _messageBoxCommand
                    ?? (_messageBoxCommand = new RelayCommand<string>(
                    obj =>
                    {
                        MessageBoxMessage.Send("Warning: TEST", obj, true);
                    },
                    obj => true));
            }
        }

        private RelayCommand _stopEditingMapCommand;
        public RelayCommand StopEditingMapCommand
        {
            get
            {
                return _stopEditingMapCommand
                    ?? (_stopEditingMapCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.StopEditingLayer(true);
                    },
                    () => true
                    ));
            }
        }

        private RelayCommand _stopSelectionMapCommand;
        public RelayCommand StopSelectionMapCommand
        {
            get
            {
                return _stopSelectionMapCommand
                    ?? (_stopSelectionMapCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.StopSelecting();
                        Messenger.Default.Send(new MapMessage() { MessageType = MapMessageType.CursorMode, CursorMode = tkCursorMode.cmPan });
                    },
                    () => true
                    ));
            }
        }

        private RelayCommand _undoEditingMapCommand;
        public RelayCommand UndoEditingCommand
        {
            get
            {
                return _undoEditingMapCommand
                    ?? (_undoEditingMapCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.Undo();
                    },
                    () => true
                    ));
            }
        }

        private RelayCommand _deleteShapeCommand;
        public RelayCommand DeleteShapeCommand
        {
            get
            {
                return _deleteShapeCommand
                    ?? (_deleteShapeCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.DeleteShape();
                        StopEditingMapCommand.Execute(null);
                    },
                    () => true
                    ));
            }
        }

        private RelayCommand _addRasterCommand;
        public RelayCommand AddRasterCommand
        {
            get
            {
                return _addRasterCommand
                    ?? (_addRasterCommand = new RelayCommand(
                    () =>
                    {
                        //MessageBoxMessage.Send("Warning: TEST", "ADD DUMMY RASTER", true);
                        string path;

                        //TODO: not mvvm!

                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Title = "Load raster file";
                        openFileDialog.Filter = $"Raster file (tif)|*tif";
                        openFileDialog.InitialDirectory = @"C:\temp2\";

                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                            path = openFileDialog.FileName;
                        else
                        {
                            //MessageBoxMessage.Send("INFO", "Loading raster file: Aborted...", true); 
                            return;
                        }

                        string layerName = Path.GetFileNameWithoutExtension(path);

                        if (MapLayers.Any(m => m.Name == layerName))
                        {
                            layerName += "_2";
                        }

                        MapControl.Tools.AddRasterLayer(path, layerName);
                        MapControl.Tools.ZoomToLayer(layerName);

                        //MapControl.Tools.AddRasterLayer(@"C:\temp2\vispa4326_.tif", "GUGUS");
                        //MapControl.Tools.ZoomToLayer("GUGUS");
                    },
                    () =>
                    {
                        //TODO
                        return true;
                    }
                    ));
            }
        }

        private RelayCommand _addDummyWMSCommand;
        public RelayCommand AddDummyWMSCommand
        {
            get
            {
                return _addDummyWMSCommand
                    ?? (_addDummyWMSCommand = new RelayCommand(
                    () =>
                    {
                        MessageBoxMessage.Send("Warning: TEST", "ADD DUMMY WMS", true);

                        Extents extents = new Extents();
                        extents.SetBounds(603076.67893, 1636591.39018, 0,
                                604831.93807, 1637912.98873, 0);
                        MapControl.Tools.AddWMSLayer("http://geoserver.geobrowser.ch/geoserver/ows", "restb:Olancho", "Olancho", extents, 32616, @"image/png");
                        MapControl.Tools.ZoomToLayer("Olancho");
                    },
                    () =>
                    {
                        //TODO
                        return true;
                    }
                    ));
            }
        }

        #endregion

        #region ProjectCommands

        private RelayCommand _createProjectCommand;
        public RelayCommand CreateProjectCommand
        {
            get
            {
                return _createProjectCommand
                    ?? (_createProjectCommand = new RelayCommand(
                    async () =>
                    {
                        NewProjectWindow.Close();

                        //Clean Up
                        SelectedHazardMap = null;
                        SelectedLayer = null;
                        SelectedMappedObject = null;
                        SelectedMergedObjectParameter = null;
                        SelectedMergedPropertyDefinitions = null;
                        TabControlSelectedIndex = 0;

                        SetupMap();
                        //Assign new project
                        Project = NewProject;

                        try
                        {
                            IsBusy = true;
                            using (ResTBContext db = new ResTBContext())
                            {
                                //get standard pra values
                                var pras = await db.StandardPrAs.ToListAsync();
                                foreach (var pra in pras)
                                {
                                    var projectPra = new PrA()
                                    {
                                        IKClassesId = pra.IKClassesId,
                                        NatHazardId = pra.NatHazardId,
                                        Value = pra.Value,
                                    };
                                    Project.PrAs.Add(projectPra);   //add pra value to project
                                }

                                db.Projects.Add(Project);
                                await db.SaveChangesAsync();
                            }
                            IsBusy = false;
                        }
                        catch (DbUpdateException ex)
                        {
                            MessageBoxMessage.Send("ERROR: DBUpdate", ex.ToString(), true);

                            //throw;
                        }
                        finally
                        {
                            IsBusy = false;
                        }

                        MapControl.Tools.AddProjectLayers(Project.Id);

                    },
                    () => NewProject != null));
            }
        }

        private RelayCommand _openProjectCommand;
        public RelayCommand OpenProjectCommand
        {
            get
            {
                return _openProjectCommand
                    ?? (_openProjectCommand = new RelayCommand(
                    () =>
                    {
                        IsBusy = true;
                        OpenProjectWindow.Close();

                        //Clean Up 
                        SelectedHazardMap = null;
                        SelectedLayer = null;
                        SelectedMappedObject = null;
                        SelectedMergedObjectParameter = null;
                        SelectedMergedPropertyDefinitions = null;
                        TabControlSelectedIndex = 0;

                        SetupMap();

                        //Assign selected project
                        Project = SelectedProject;

                        MapControl.Tools.AddProjectLayers(Project.Id);

                        if (HasPerimeter)
                        {
                            MapControl.Tools.ZoomToLayer(MapControl.Tools.GetLayerNamesFromPostGISType(ResTBPostGISType.Perimeter).First());
                        }

                        IsBusy = false;
                    },
                    () => SelectedProject != null
                    ));
            }
        }
        private RelayCommand _deleteProjectCommand;
        public RelayCommand DeleteProjectCommand
        {
            get
            {
                return _deleteProjectCommand
                    ?? (_deleteProjectCommand = new RelayCommand(
                    async () =>
                    {
                        //if (SelectedProject == Project)
                        //    return;

                        bool receivedCallback = false;
                        var result = MessageBoxResult.None;

                        Action<MessageBoxResult> callback = r =>
                        {
                            receivedCallback = true;
                            result = r;
                        };

                        var message = new DialogMessage(this, $"{Resources.Msg_Question}",
                            $"{Resources.Msg_QuestionDeleteProject}\n\n" +
                            $"{Resources.Project_Name}:  \t{SelectedProject.Name}\n" +
                            $"{Resources.Project_Number}:\t{SelectedProject.Number}\n\n" +
                            $"{Resources.Msg_QuestionContinue}", callback)
                        {
                            Icon = MessageBoxImage.Question,
                            Button = MessageBoxButton.YesNo,
                            DefaultResult = MessageBoxResult.No
                        };

                        Messenger.Default.Send(message, WindowType.OpenProjectWindow);

                        if (result != MessageBoxResult.Yes || !receivedCallback)
                        {
                            return;
                        }
                        else
                        {
                            IsBusy = true;
                            int returnDelete = -1;

                            // delete Project
                            string query = $"select restb_deleteproject({SelectedProject.Id})";
                            using (ResTBContext db = new ResTBContext())
                            {
                                returnDelete = await db.Database.ExecuteSqlCommandAsync(query);

                                //Update the projects list
                                AllProjects = new ObservableCollection<Project>(
                                    db.Projects
                                        .Include(p => p.ProtectionMeasure)
                                        .OrderBy(p => p.Name).ThenBy(p => p.Number)
                                        .ToList()
                                    );
                            }

                            //TODO: Return value from delete function isn't received. No check possible...

                            //if (returnDelete == 1)
                            //{
                            //    //Change selectedProject
                            //    SelectedProject = AllProjects.First();
                            //}
                            //else
                            //{
                            //    MessageBoxMessage.Send("Error", "Project could not be deleted from database", true);
                            //}

                            //Change selectedProject
                            SelectedProject = AllProjects.FirstOrDefault();

                            IsBusy = false;
                            return;
                        }

                    },
                    () => SelectedProject != null && SelectedProject?.Id != Project?.Id
                    ));
            }
        }

        private RelayCommand _exportProjectCommand;
        public RelayCommand ExportProjectCommand
        {
            get
            {
                return _exportProjectCommand
                    ?? (_exportProjectCommand = new RelayCommand(
                    () =>
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.DefaultExt = ".restb";
                        saveFileDialog.Filter = $"{Resources.App_Files} (.zip)|*.zip";
                        saveFileDialog.Title = $"{Resources.Project_Export}";
                        saveFileDialog.FileName = SerializationHelper.makeValidFileName(Project.Name);
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            MapControl.Tools.ExportProject(SelectedProject.Id, saveFileDialog.FileName);
                    },
                    () => Project != null && HasDBConnection && !IsBackgroundBusy && !IsBusy
                    ));
            }
        }


        private RelayCommand _importProjectCommand;
        public RelayCommand ImportProjectCommand
        {
            get
            {
                return _importProjectCommand
                    ?? (_importProjectCommand = new RelayCommand(
                    () =>
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.DefaultExt = ".restb";
                        openFileDialog.Filter = $"{Resources.App_Files} (.zip)|*.zip";
                        openFileDialog.Title = $"{Resources.Project_Import}";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            int projectID = MapControl.Tools.ImportProject(openFileDialog.FileName);

                            if (projectID > 0)
                            {
                                IsBusy = true;
                                using (ResTBContext context = new ResTBContext())
                                {
                                    SelectedProject = context.Projects.Find(projectID);

                                    //Clean Up 
                                    SelectedHazardMap = null;
                                    SelectedLayer = null;
                                    SelectedMappedObject = null;
                                    SelectedMergedObjectParameter = null;
                                    SelectedMergedPropertyDefinitions = null;
                                    TabControlSelectedIndex = 0;

                                    SetupMap();

                                    //Assign selected project
                                    Project = SelectedProject;

                                    MapControl.Tools.AddProjectLayers(Project.Id);

                                    if (HasPerimeter)
                                    {
                                        MapControl.Tools.ZoomToLayer(MapControl.Tools.GetLayerNamesFromPostGISType(ResTBPostGISType.Perimeter).First());
                                    }
                                }
                            }
                            IsBusy = false;

                        }
                    },
                    () => HasDBConnection && !IsBackgroundBusy && !IsBusy
                    ));
            }
        }

        private RelayCommand _printCommand;
        public RelayCommand PrintCommand
        {
            get
            {
                return _printCommand
                    ?? (_printCommand = new RelayCommand(
                    () =>
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.DefaultExt = ".pdf";
                        saveFileDialog.Filter = "PDF (.pdf)|*.pdf";
                        saveFileDialog.Title = $"{Resources.Project_ExportPDF}";
                        saveFileDialog.FileName = SerializationHelper.makeValidFileName(Project.Name);
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            MapControl.Tools.PrintAsPDF(saveFileDialog.FileName, Project);
                    },
                    () => Project != null && HasDBConnection && !IsBackgroundBusy && !IsBusy
                    ));
            }
        }

        private RelayCommand _updateProjectCommand;
        public RelayCommand UpdateProjectCommand
        {
            get
            {
                return _updateProjectCommand
                    ?? (_updateProjectCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            IsBusy = true;
                            using (ResTBContext db = new DB.ResTBContext())
                            {
                                var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == Project.Id);
                                if (project != null)
                                {
                                    if (project.Name != Project.Name ||
                                        project.Number != Project.Number ||
                                        project.Description != Project.Description)
                                    {
                                        project.Name = Project.Name;
                                        project.Number = Project.Number;
                                        project.Description = Project.Description;
                                        await db.SaveChangesAsync();
                                    }
                                    //else
                                    //    MessageBoxMessage.Send("Warning: Update Project", "No project changes found!", true);  
                                }
                                else
                                {
                                    MessageBoxMessage.Send("ERROR: Update Project", "Project not found", true);  //TODO: Translation
                                }
                            }
                            IsBusy = false;

                            //TODO: add indicator for successfull saving

                        }
                        catch (DbUpdateException ex)
                        {
                            MessageBoxMessage.Send("ERROR: DBUpdate", ex.ToString(), true);  //TODO: Translation
                                                                                             //throw;
                        }
                        finally
                        {
                            IsBusy = false;
                        }

                    },
                    () => true));
            }
        }

        private RelayCommand _newProjectWinCommand;
        public RelayCommand NewProjectWinCommand
        {
            get
            {
                return _newProjectWinCommand
                    ?? (_newProjectWinCommand = new RelayCommand(
                    () =>
                    {
                        //MessageBoxMessage.Send("Warning: TEST", "NEW PROJECT COMMAND", true);

                        NewProject = new Project()
                        {
                            Name = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Number = "0",
                            ProtectionMeasure = new ProtectionMeasure(),
                        };

                        NewProjectWindow = new NewProjectWindow();
                        //NewProjectWindow.Topmost = true;
                        NewProjectWindow.ShowDialog();
                    },
                    () => HasDBConnection && !IsBackgroundBusy && !IsBusy
                    ));
            }
        }

        private RelayCommand _openProjectWinCommand;
        public RelayCommand OpenProjectWinCommand
        {
            get
            {
                return _openProjectWinCommand
                    ?? (_openProjectWinCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            IsBusy = true;

                            using (ResTBContext db = new DB.ResTBContext())
                            {
                                AllProjects = new ObservableCollection<Project>(
                                    await db.Projects
                                        .Include(p => p.ProtectionMeasure)
                                        .OrderBy(p => p.Name).ThenBy(p => p.Number)
                                        .ToListAsync()
                                    );
                            }
                            if (AllProjects.Any())
                            {
                                SelectedProject = AllProjects.First();
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            IsBusy = false;
                        }

                        OpenProjectWindow = new OpenProjectWindow();
                        //OpenProjectWindow.Topmost = true;
                        OpenProjectWindow.ShowDialog();
                    },
                    () => HasDBConnection && !IsBackgroundBusy && !IsBusy
                    ));
            }
        }
        #endregion

        #region PerimeterCommands

        private RelayCommand _addPerimeterCommand;
        public RelayCommand AddPerimeterCommand
        {
            get
            {
                return _addPerimeterCommand
                    ?? (_addPerimeterCommand = new RelayCommand(
                    () =>
                    {
                        //MessageBoxMessage.Send("Warning: TEST", "ADD PERIMETER", true);
                        if (HasPerimeter)
                        {
                            MapControl.Tools.StartEditingLayer(MapControl.Tools.GetLayerNamesFromPostGISType(ResTBPostGISType.Perimeter).FirstOrDefault());
                            MapControl.Tools.AddShape(true);
                        }
                    },
                    () =>
                    {
                        if ((MapLayers.Count > 0) && (MapLayers.First().Children != null))
                        {
                            List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());

                            //trigger Button disable for shapecount==0
                            return HasPerimeter && (allSubLayers?
                        .Where(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.Perimeter)
                        .SingleOrDefault()?.Layer.ShapeCount == 0);
                        }
                        else return false;


                    }));
            }
        }
        private RelayCommand _editPerimeterCommand;
        public RelayCommand EditPerimeterCommand
        {
            get
            {
                return _editPerimeterCommand
                    ?? (_editPerimeterCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.ZoomToLayer(ResTB.Translation.Properties.Resources.Perimeter);

                        //MessageBoxMessage.Send("Warning: TEST", "EDIT PERIMETER START", true);
                        MapControl.Tools.StartEditingLayer(ResTB.Translation.Properties.Resources.Perimeter);
                        MapControl.Tools.EditShape();

                    },
                    () =>
                    {
                        if ((MapLayers.Count > 0) && (MapLayers.First().Children != null))
                        {
                            List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());

                            //TODO: trigger Button disable for shapecount missing
                            return allSubLayers?
                        .Where(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.Perimeter)
                        .SingleOrDefault()?.Layer.ShapeCount == 1;
                        }
                        else return false;
                    }));
            }
        }

        #endregion //PerimeterCommands

        #region HazardMapsCommand    

        private RelayCommand<bool> _createHazardMapCommand;
        public RelayCommand<bool> CreateHazardMapCommand
        {
            get
            {
                return _createHazardMapCommand
                    ?? (_createHazardMapCommand = new RelayCommand<bool>(
                    beforeMeasure =>
                    {
                        try
                        {
                            //IsBusy = true;

                            SelectedHazardMap = null;

                            using (ResTBContext db = new DB.ResTBContext())
                            {
                                ResTBHazardMapLayer hazard = new ResTBHazardMapLayer(
                                    Project.Id, beforeMeasure, SelectedNatHazard, SelectedHazardIndex.Index
                                    );

                                MapControl.Tools.StartEditingLayer(hazard);
                                MapControl.Tools.AddShape(true);
                            }

                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        finally
                        {
                            //IsBusy = false;
                        }

                    },
                    beforeMeasure => HasDBConnection && SelectedHazardIndex != null && SelectedNatHazard != null
                    ));
            }
        }

        private RelayCommand<bool> _selectedHazardMapCommand;
        public RelayCommand<bool> SelectHazardMapCommand
        {
            get
            {
                return _selectedHazardMapCommand
                    ?? (_selectedHazardMapCommand = new RelayCommand<bool>(

                    beforeMeasure =>
                    {
                        if (beforeMeasure)
                            MapControl.Tools.StartSelecting(ResTBPostGISType.HazardMapBefore);
                        else
                            MapControl.Tools.StartSelecting(ResTBPostGISType.HazardMapAfter);

                    },
                    beforeMeasure =>
                    {
                        List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());

                        LayersModel layerModel;
                        if (beforeMeasure)
                        {
                            layerModel = allSubLayers?
                                .FirstOrDefault(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.HazardMapBefore);
                        }
                        else
                        {
                            layerModel = allSubLayers?
                               .FirstOrDefault(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.HazardMapAfter);
                        }

                        if (layerModel != null && layerModel.Layer != null)
                        {
                            return layerModel.Layer.ShapeCount > 0;
                        }
                        else return false;

                    }));
            }
        }

        private RelayCommand<bool> _editHazardMapCommand;
        public RelayCommand<bool> EditHazardMapCommand
        {
            get
            {
                return _editHazardMapCommand
                    ?? (_editHazardMapCommand = new RelayCommand<bool>(
                    beforeMeasure =>
                    {
                        ResTBHazardMapLayer hazard;
                        hazard = new ResTBHazardMapLayer(Project.Id, beforeMeasure, SelectedNatHazard, SelectedHazardIndex.Index);
                        MapControl.Tools.StartEditingLayer(hazard);
                        MapControl.Tools.EditShape(SelectedShapeIndex);
                    },
                    beforeMeasure =>
                    {
                        return SelectedHazardMap != null;
                    }));
            }
        }

        private RelayCommand _updateSelectedHazardMapCommand;
        public RelayCommand UpdateSelectedHazardMapCommand  //todo: generalize for all db changes
        {
            get
            {
                return _updateSelectedHazardMapCommand
                    ?? (_updateSelectedHazardMapCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            IsBusy = true;
                            using (ResTBContext db = new DB.ResTBContext())
                            {
                                HazardMap hazMap = await db.HazardMaps.Include(h => h.NatHazard).Where(m => m.ID == SelectedHazardMap.ID).FirstOrDefaultAsync();
                                hazMap.Index = SelectedHazardMap.Index;
                                hazMap.BeforeAction = SelectedHazardMap.BeforeAction;

                                await db.SaveChangesAsync();   //todo: check if selected hazard map is updated in db!

                                // Maybe we have a new HazardMap Layer
                                ResTBHazardMapLayer hazardLayer = new ResTBHazardMapLayer(Project.Id, hazMap.BeforeAction, hazMap.NatHazard, hazMap.Index);
                                MapControl.Tools.AddProjectLayer(hazardLayer);

                            }
                            MapControl.Tools.Redraw(true);
                            IsBusy = false;
                        }
                        catch (DbUpdateException ex)
                        {
                            MessageBoxMessage.Send("ERROR: DBUpdate", ex.ToString(), true);
                            //throw;
                        }
                        finally
                        {
                            IsBusy = false;
                        }

                    },
                    () => SelectedHazardMap != null
                    ));
            }
        }
        #endregion //HazardMapsCommand    

        #region MitigationMeassure_Commands

        private RelayCommand _addMitigationCommand;
        public RelayCommand AddMitigationCommand
        {
            get
            {
                return _addMitigationCommand
                    ?? (_addMitigationCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.StartEditingLayer(MapControl.Tools.GetLayerNamesFromPostGISType(ResTBPostGISType.MitigationMeasure).FirstOrDefault());
                        MapControl.Tools.AddShape(true);
                    },
                    () =>
                    {
                        return true;
                    }));
            }
        }

        private RelayCommand _editMitigationCommand;
        public RelayCommand EditMitigationCommand
        {
            get
            {
                return _editMitigationCommand
                    ?? (_editMitigationCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.StartEditingLayer(MapControl.Tools.GetLayerNamesFromPostGISType(ResTBPostGISType.MitigationMeasure).FirstOrDefault());
                        MapControl.Tools.EditShape();

                    },
                    () =>
                    {
                        List<LayersModel> allSubLayers = MapLayers.FirstOrDefault()?.getAllChildren(MapLayers.FirstOrDefault()?.Children.ToList());

                        LayersModel layerModel = allSubLayers?
                            .FirstOrDefault(m => m.Layer != null && m.Layer.LayerType == LayerType.ProjectLayer && ((ResTBPostGISLayer)m.Layer).ResTBPostGISType == ResTBPostGISType.MitigationMeasure);

                        if (layerModel != null && layerModel.Layer != null)
                        {
                            return layerModel.Layer.ShapeCount > 0;
                        }
                        else return false;

                    }));
            }
        }

        private RelayCommand _updateMitigationMeasureCommand;
        public RelayCommand UpdateMitigationMeasureCommand
        {
            get
            {
                return _updateMitigationMeasureCommand
                    ?? (_updateMitigationMeasureCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            IsBusy = true;
                            using (ResTBContext db = new DB.ResTBContext())
                            {
                                ProtectionMeasure protection = await db.ProtectionMeasurements.FindAsync(Project.ProtectionMeasure.ID);

                                if (protection != null)
                                {
                                    //TODO: IMPLEMENT UPDATE
                                    protection.Description = Project.ProtectionMeasure.Description;
                                    protection.Costs = Project.ProtectionMeasure.Costs;
                                    protection.LifeSpan = Project.ProtectionMeasure.LifeSpan;
                                    protection.OperatingCosts = Project.ProtectionMeasure.OperatingCosts;
                                    protection.MaintenanceCosts = Project.ProtectionMeasure.MaintenanceCosts;
                                    protection.RateOfReturn = Project.ProtectionMeasure.RateOfReturn;
                                    protection.ValueAddedTax = Project.ProtectionMeasure.ValueAddedTax;

                                    await db.SaveChangesAsync();
                                }
                            }
                            IsBusy = false;

                            //TODO: add indicator for successfull saving

                        }
                        catch (DbUpdateException ex)
                        {
                            MessageBoxMessage.Send("ERROR: DBUpdate", ex.ToString(), true);
                            //throw;
                        }
                        finally
                        {
                            IsBusy = false;
                        }

                    },
                    () => true));
            }
        }

        #endregion

        #region DamagePotentialCommands

        private RelayCommand _addDamagePotentialCommand;
        public RelayCommand AddDamagePotentialCommand
        {
            get
            {
                return _addDamagePotentialCommand
                    ?? (_addDamagePotentialCommand = new RelayCommand(
                    () =>
                    {
                        SelectedMappedObject = null;

                        MapControl.Tools.StartEditingLayer(SelectedObjectParameter, true);
                        MapControl.Tools.AddShape(false);
                    },
                    () =>
                    {
                        return true;
                    }));
            }
        }

        private RelayCommand _selectDamagePotentialCommand;
        public RelayCommand SelectDamagePotentialCommand
        {
            get
            {
                return _selectDamagePotentialCommand
                    ?? (_selectDamagePotentialCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.StartSelecting(ResTBPostGISType.DamagePotential);
                    },
                    () =>
                    {
                        return true;
                    }));
            }
        }

        private RelayCommand _editDamagePotentialCommand;
        public RelayCommand EditDamagePotentialCommand
        {
            get
            {
                return _editDamagePotentialCommand
                    ?? (_editDamagePotentialCommand = new RelayCommand(
                    () =>
                    {
                        //MessageBoxMessage.Send("ERROR", "NOT IMPLEMENTED", true);

                        MapControl.Tools.StartEditingLayer(SelectedObjectParameter, true);
                        MapControl.Tools.EditShape(SelectedShapeIndex);
                    },
                    () =>
                    {
                        return SelectedMappedObject != null;
                    }));
            }
        }

        private RelayCommand _saveMappedObjectCommand;
        public RelayCommand SaveMappedObjectCommand
        {
            get
            {
                return _saveMappedObjectCommand
                    ?? (_saveMappedObjectCommand = new RelayCommand(
                    async () =>
                    {
                        IsBusy = true;

                        if (SelectedMappedObject == null)
                        {
                            throw new ArgumentNullException(nameof(SelectedMappedObject), "selected mapped object is null");
                        }

                        MappedObject mappedObject = null;
                        using (ResTBContext db = new ResTBContext())
                        {
                            mappedObject = await db.MappedObjects
                                                .Include(m => m.Objectparameter.MotherOtbjectparameter.HasProperties)
                                                .Include(m => m.Objectparameter.HasProperties)
                                                .Include(m => m.FreeFillParameter)
                                                .Include(m => m.Objectparameter.ObjectClass)
                                                .Include(m => m.ResilienceValues)
                                                .Where(m => m.ID == SelectedMappedObject.ID).FirstOrDefaultAsync();

                            if (mappedObject == null)
                            {
                                throw new ArgumentNullException(nameof(mappedObject), "selected mapped object not found in database");
                            }

                            List<ObjectparameterHasProperties> hasProps = new List<ObjectparameterHasProperties>();
                            if (mappedObject.Objectparameter.MotherOtbjectparameter != null)
                                hasProps = mappedObject.Objectparameter.MotherOtbjectparameter.HasProperties;
                            else
                                hasProps = mappedObject.Objectparameter.HasProperties;

                            foreach (ObjectparameterHasProperties ophp in hasProps)
                            {
                                Debug.WriteLine($"{ophp.Property}, optional: {ophp.isOptional}");

                                //get property from viewmodel object
                                var newProperty = ophp.Property.Split('.').Select(s => SelectedMergedObjectParameter.GetType().GetProperty(s)).FirstOrDefault();
                                //get property from database object
                                var oldProperty = ophp.Property.Split('.').Select(s => mappedObject.Objectparameter.GetType().GetProperty(s)).FirstOrDefault();
                                if (oldProperty == null)
                                {
                                    Debug.WriteLine($"ERROR: property {ophp.Property} not found in mapped object");
                                    continue;
                                }

                                if (ophp.isOptional)    //free fill parameter
                                {
                                    var newValue = newProperty.GetValue(SelectedMergedObjectParameter, null);

                                    if (mappedObject.FreeFillParameter == null)
                                    {
                                        mappedObject.FreeFillParameter = new Objectparameter();
                                        db.Entry(mappedObject.FreeFillParameter).State = EntityState.Added;
                                        await db.SaveChangesAsync();               //TODO: FIX ERROR   <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                                    }

                                    var oldValue = oldProperty.GetValue(mappedObject.FreeFillParameter, null);

                                    if (newValue != null && oldValue != null && !newValue.Equals(oldValue))
                                    {
                                        Debug.WriteLine($"old: {oldValue} , new: {newValue}");

                                        oldProperty.SetValue(mappedObject.FreeFillParameter, newValue);
                                        db.Entry(mappedObject.FreeFillParameter).State = EntityState.Modified;
                                        await db.SaveChangesAsync();
                                    }
                                }
                                else    //change default values
                                {
                                    try
                                    {
                                        var newValue = newProperty.GetValue(SelectedMergedObjectParameter, null);
                                        var oldValue = oldProperty.GetValue(mappedObject.Objectparameter, null);

                                        bool isAlreadyCloned = (mappedObject.Objectparameter.MotherOtbjectparameter != null);

                                        if (newValue != null && oldValue != null && !newValue.Equals(oldValue))
                                        {
                                            // If it was a standard Object and we changed something on the standard
                                            if ((mappedObject.Objectparameter.IsStandard) && (!isAlreadyCloned))
                                            {
                                                mappedObject.Objectparameter = Objectparameter.CloneObject(mappedObject.Objectparameter);
                                                db.Entry(mappedObject.Objectparameter).State = EntityState.Added;
                                                await db.SaveChangesAsync();
                                            }
                                            oldProperty.SetValue(mappedObject.Objectparameter, newValue);
                                            db.Entry(mappedObject.Objectparameter).State = EntityState.Modified;
                                            await db.SaveChangesAsync();
                                        }
                                    }
                                    catch (InvalidOperationException e)
                                    {
                                        throw new InvalidOperationException($"Could not set value for {ophp.Property}", e);
                                    }
                                }

                            }
                        }

                        MapControl.Tools.Redraw(true);

                        //Rebuild and reload Merged object
                        MergeSelectedMappedObject();

                        IsBusy = false;
                    },
                    () =>
                    {
                        return SelectedMergedObjectParameter != null;
                    }));
            }
        }



        #endregion

        #region ResilienceCommands
        //SelectResilienceCommand


        private RelayCommand<bool> _selectResilienceCommand;
        public RelayCommand<bool> SelectResilienceCommand
        {
            get
            {
                return _selectResilienceCommand
                    ?? (_selectResilienceCommand = new RelayCommand<bool>(
                    beforeMeasure =>
                    {
                        //MessageBoxMessage.Send("Error", "Selektion funktioniert nicht.... ", false);

                        if (beforeMeasure)
                            MapControl.Tools.StartSelecting(ResTBPostGISType.ResilienceBefore);
                        else
                            MapControl.Tools.StartSelecting(ResTBPostGISType.ResilienceAfter);
                    },
                    beforeMeasure => true
                    ));
            }
        }

        private RelayCommand<bool> _saveResilienceCommand;
        public RelayCommand<bool> SaveResilienceCommand
        {
            get
            {
                return _saveResilienceCommand
                    ?? (_saveResilienceCommand = new RelayCommand<bool>(
                    async beforeMeasure =>
                    {
                        await SaveResilienceAsync(beforeMeasure);

                        MergeSelectedMappedObject();
                        MapControl.Tools.Redraw(true);
                        //await Task.Run(() => MapControl.Tools.Redraw(true));
                    },
                    beforeMeasure => true
                    ));
            }
        }

        private async Task SaveResilienceAsync(bool beforeMeasure)
        {
            MappedObject mappedObject;
            using (ResTBContext db = new ResTBContext())
            {
                mappedObject = await db.MappedObjects
                    .Include(m => m.ResilienceValues)
                    .Where(m => m.ID == SelectedMappedObject.ID)
                    .SingleOrDefaultAsync();
                if (mappedObject != null)
                {
                    List<ResilienceValues> mergedResilienceValues;
                    if (beforeMeasure)
                    {
                        mergedResilienceValues = SelectedMergedObjectParameter.ResilienceValuesMergedBefore.ToList();
                    }
                    else
                    {
                        mergedResilienceValues = SelectedMergedObjectParameter.ResilienceValuesMergedAfter.ToList();
                    }


                    if (beforeMeasure == true)
                    {
                        var resValToDeleteAll = db.ResilienceValues.Where(rv => rv.MappedObject.ID == SelectedMappedObject.ID);
                        db.ResilienceValues.RemoveRange(resValToDeleteAll);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var resValToDelete = db.ResilienceValues.Where(rv => rv.MappedObject.ID == SelectedMappedObject.ID &&
                                                                         rv.ResilienceWeight.BeforeAction == beforeMeasure);
                        db.ResilienceValues.RemoveRange(resValToDelete);
                        await db.SaveChangesAsync();
                    }

                    foreach (ResilienceValues resVal in mergedResilienceValues)
                    {
                        ResilienceWeight resWeight = await db.ResilienceWeights
                                                        .Include(r => r.ResilienceFactor)
                                                        .Include(r => r.NatHazard)
                                                        .Where(r => r.ID == resVal.ResilienceWeight.ID)
                                                        .SingleOrDefaultAsync();
                        if (resWeight != null)
                        {
                            db.ResilienceValues.Add(
                                new ResilienceValues()
                                {
                                    MappedObject = mappedObject,
                                    OverwrittenWeight = resVal.OverwrittenWeight,
                                    Value = resVal.Value,
                                    ResilienceWeight = resWeight
                                }
                            );
                        }
                        else
                        {
#if DEBUG
                                        throw new ArgumentNullException(nameof(resWeight));
#endif
                        }

                        if (beforeMeasure)  // do it also for after mitigation
                        {
                            ResilienceWeight resWeightAfter = await db.ResilienceWeights
                                                                .Where(r => r.ResilienceFactor.ID == resWeight.ResilienceFactor.ID &&
                                                                            r.NatHazard.ID == resWeight.NatHazard.ID &&
                                                                            r.BeforeAction == false)
                                                                .SingleOrDefaultAsync();

                            if (resWeightAfter != null)
                            {
                                db.ResilienceValues.Add(
                                    new ResilienceValues()
                                    {
                                        MappedObject = mappedObject,
                                        OverwrittenWeight = resVal.OverwrittenWeight,
                                        Value = resVal.Value,
                                        ResilienceWeight = resWeightAfter
                                    }
                                );
                            }
                            else
                            {
#if DEBUG
                                            throw new ArgumentNullException(nameof(resWeightAfter));
#endif
                            }
                        }
                    }
                    await db.SaveChangesAsync();

                }
            }//end of db context

        }

        private RelayCommand _copyResilienceCommand;
        /// <summary>
        /// Copy Resilience Values from beforeAction to afterAction
        /// </summary>
        public RelayCommand CopyResilienceCommand
        {
            get
            {
                return _copyResilienceCommand
                    ?? (_copyResilienceCommand = new RelayCommand(
                    async () =>
                    {
                        MapControl.Tools.StopSelecting();
                        //Save resiliece values first
                        await SaveResilienceAsync(true);
                        MergeSelectedMappedObject();
                        //SaveResilienceCommand.Execute(true);

                        IsCopyingResilience = true;

                        MapControl.Tools.StartSelecting(ResTBPostGISType.ResilienceBefore);

                        //Messenger.Default.Send(new MapMessage() { MessageType = MapMessageType.CursorMode, CursorMode = tkCursorMode.cmIdentify });
                    },
                    () => SelectedMappedObject != null
                    ));
            }
        }

        private RelayCommand<bool> _copyAllResilienceCommand;
        /// <summary>
        /// Copy resiliences values from beforeAction to afterAction for all mapped objects
        /// </summary>
        public RelayCommand<bool> CopyAllResilienceCommand
        {
            get
            {
                return _copyAllResilienceCommand
                    ?? (_copyAllResilienceCommand = new RelayCommand<bool>(
                    async onlyAfterAction =>
                    {
                        MapControl.Tools.StopSelecting();
                        //Save resiliece values first
                        await SaveResilienceAsync(!onlyAfterAction);
                        MergeSelectedMappedObject();
                        //SaveResilienceCommand.Execute(!onlyAfterAction);

                        //DIALOG
                        bool receivedCallback = false;
                        var result = MessageBoxResult.None;

                        Action<MessageBoxResult> callback = r =>
                        {
                            receivedCallback = true;
                            result = r;
                        };

                        string messageString = $"{Resources.Resilience_CopyAll}.\n\n";
                        if (onlyAfterAction)
                            messageString += $"{Resources.Resilience_CopyDetail_After}";
                        else
                            messageString += $"{Resources.Resilience_CopyDetail_Before}";
                        messageString += $"\n\n{Resources.Msg_QuestionContinue}";
                        var message = new DialogMessage(this, $"{Resources.Msg_Question}", messageString, callback)
                        {
                            Icon = MessageBoxImage.Question,
                            Button = MessageBoxButton.YesNo,
                            DefaultResult = MessageBoxResult.No
                        };

                        Messenger.Default.Send(message, WindowType.MainWindow);

                        if (result != MessageBoxResult.Yes || !receivedCallback)
                        {
                            return;
                        }
                        else
                        {

                            IsBusy = true;
                            StatusBarProgressBarVisible = true;
                            StatusBarProgressBarMessageString = "";
                            StatusBarProgressBarPercent = 0;
                            StatusBarMainString = "";

                            //Get all MappedObject ID
                            List<int> mappedObjIds = null;
                            using (ResTBContext db = new DB.ResTBContext())
                            {
                                mappedObjIds = await db.MappedObjects.AsNoTracking()
                                    .Where(m => m.Project.Id == Project.Id)
                                    .Select(m => m.ID)
                                    .OrderBy(id => id)
                                    .ToListAsync();
                            }
                            int idCount = mappedObjIds.Count() - 1;
                            int i = 0;
                            foreach (int id in mappedObjIds)
                            {
                                if (SelectedMappedObject.ID != id)
                                {
                                    i++;

                                    StatusBarProgressBarPercent = 100 / idCount * i;
                                    StatusBarProgressBarMessageString = $"{StatusBarProgressBarPercent:F0}%";
                                    await MappedObjectManager.CopyResilience(SelectedMappedObject.ID, id, onlyAfterAction);
                                }
                            }

                            IsBusy = false;
                            StatusBarProgressBarVisible = false;
                            StatusBarProgressBarMessageString = "";
                            StatusBarProgressBarPercent = 100;

                            MapControl.Tools.Redraw(true);
                        }
                    },
                    onlyAfterAction => SelectedMappedObject != null
                    ));
            }
        }




        #endregion //ResilienceCommands

        #region LayerCommands
        private RelayCommand<LayerMoveType> _moveLayerCommand;
        public RelayCommand<LayerMoveType> MoveLayerCommand
        {
            get
            {
                return _moveLayerCommand
                    ?? (_moveLayerCommand = new RelayCommand<LayerMoveType>(
                    moveType =>
                    {
                        if (SelectedLayer != null)
                        {
                            //MessageBoxMessage.Send("WARNING", moveType.ToString() + "\n" + SelectedLayer.Name, true);
                            int stepsUp = 1;
                            List<LayersModel> modelsToMove = SelectedLayer.getLayersToMoveUp(out stepsUp);

                            for (int i = 0; i < stepsUp; i++)
                            {
                                foreach (LayersModel lm in modelsToMove)
                                {
                                    MapControl.Tools.SetLayerPosition(lm.Layer?.Name, moveType);
                                }
                            }


                        }
                    },
                    moveType => true//SelectedLayer != null
                    ));
            }
        }


        private RelayCommand<bool> _setLayerVisibleCommand;
        public RelayCommand<bool> SetLayerVisibleCommand
        {
            get
            {
                return _setLayerVisibleCommand
                    ?? (_setLayerVisibleCommand = new RelayCommand<bool>(
                    makeVisible =>
                    {
                        if (SelectedLayer != null && SelectedLayer.Layer != null)
                        {
                            //MessageBoxMessage.Send("WARNING", makeVisible.ToString() + "\n" + SelectedLayer.Name, true);

                            MapControl.Tools.SetLayerVisible(SelectedLayer.Layer.Name, makeVisible);

                            ////TODO: automatize?
                            //UpdateMapLayers();
                        }
                    },
                    makeVisible => true//SelectedLayer != null
                    ));
            }
        }

        private RelayCommand _changeLayerVisibilityCommand;
        public RelayCommand ChangeLayerVisibilityCommand
        {
            get
            {
                return _changeLayerVisibilityCommand
                    ?? (_changeLayerVisibilityCommand = new RelayCommand(
                    () =>
                    {
                        MapControl.Tools.MapControl_LayerChange -= MapControl_LayerChange;
                        foreach (LayersModel lm in MapLayers)
                        {
                            lm.MakeLayerVisible(MapControl.Tools);
                        }

                        MapControl.Tools.MapControl_LayerChange += MapControl_LayerChange;
                    },
                    () => MapLayers.Count > 0
                    ));
            }
        }


        private RelayCommand _centerLayerCommand;
        public RelayCommand CenterLayerCommand
        {
            get
            {
                return _centerLayerCommand
                    ?? (_centerLayerCommand = new RelayCommand(
                    () =>
                    {
                        if (SelectedLayer != null)
                        {
                            //MessageBoxMessage.Send("WARNING", "Center on Layer" + "\n" + SelectedLayer.Name, true);

                            MapControl.Tools.ZoomToLayer(SelectedLayer.Layer?.Name);
                        }
                    },
                    () => SelectedLayer != null && SelectedLayer.Layer != null
                    ));
            }
        }

        private RelayCommand _removeLayerCommand;
        public RelayCommand RemoveLayerCommand
        {
            get
            {
                return _removeLayerCommand
                    ?? (_removeLayerCommand = new RelayCommand(
                    () =>
                    {
                        if (SelectedLayer != null)
                        {
                            //MessageBoxMessage.Send("WARNING", "Remove Layer" + "\n" + SelectedLayer.Name, true);

                            MapControl.Tools.RemoveLayer(SelectedLayer.Layer?.Name);
                        }
                    },
                    () => SelectedLayer != null && SelectedLayer.Layer != null && SelectedLayer.Layer?.LayerType != LayerType.ProjectLayer
                    ));
            }
        }
        #endregion //LayerCommands

        #region ToolColumnCommands

        private RelayCommand _hideToolColumnCommand;
        public RelayCommand HideToolColumnCommand
        {
            get
            {
                return _hideToolColumnCommand
                    ?? (_hideToolColumnCommand = new RelayCommand(
                    () =>
                    {
                        ShowToolColumn = !ShowToolColumn;
                    },
                    () => true
                    ));
            }
        }

        private RelayCommand _placeFilterStringResetCommand;
        public RelayCommand PlaceFilterStringResetCommand
        {
            get
            {
                return _placeFilterStringResetCommand
                    ?? (_placeFilterStringResetCommand = new RelayCommand(
                    () =>
                    {
                        PlaceFilterString = "";
                    },
                    () => true
                    ));
            }
        }

        private RelayCommand _goToCoordinatesCommand;

        public RelayCommand GoToCoordinatesCommand
        {
            get
            {
                return _goToCoordinatesCommand
                    ?? (_goToCoordinatesCommand = new RelayCommand(
                    () =>
                    {
                        if (SelectedPlace != null)
                        {
                            //MessageBoxMessage.Send(SelectedPlace.name, $"{SelectedPlace.longitude} / {SelectedPlace.latitude}", true);
                            StatusBarMainString = $"{SelectedPlace.name}: {SelectedPlace.longitude:F3} / {SelectedPlace.latitude:F3}";

                            var center = new PointClass() { x = SelectedPlace.longitude, y = SelectedPlace.latitude };
                            var extent = new ExtentsClass();
                            extent.MoveTo(SelectedPlace.longitude, SelectedPlace.latitude);
                            MapControl.Tools.AxMap.SetGeographicExtents(extent);
                            MapControl.Tools.AxMap.ZoomToTileLevel(15);

                            //MapControl.Tools.AxMap.SetGeographicExtents2(SelectedPlace.longitude, SelectedPlace.latitude, 1); //NOT WORKING IN RELEASE !?
                        }
                    },
                    () => SelectedPlace != null
                    ));
            }
        }

        private RelayCommand _goToCoordinates2Command;

        public RelayCommand GoToCoordinates2Command
        {
            get
            {
                return _goToCoordinates2Command
                    ?? (_goToCoordinates2Command = new RelayCommand(
                    () =>
                    {
                        if (GoToCoordinates != null)
                        {
                            StatusBarMainString = $"{GoToCoordinates.name}: {GoToCoordinates.longitude:F3} / {GoToCoordinates.latitude:F3}";

                            var center = new PointClass() { x = GoToCoordinates.longitude, y = GoToCoordinates.latitude };
                            var extent = new ExtentsClass();
                            extent.MoveTo(GoToCoordinates.longitude, GoToCoordinates.latitude);
                            MapControl.Tools.AxMap.SetGeographicExtents(extent);
                            MapControl.Tools.AxMap.ZoomToTileLevel(15);
                        }
                    },
                    () => true
                    ));
            }
        }

        #endregion

        /// Event Handlers
        #region EventHandling

        private void MapControl_Error(object sender, Map.Events.MapControl_Error e)
        {
#if DEBUG
            MessageBoxMessage.Send("Error in " + e.InMethod, "[" + (int)e.ErrorCode + "] " + e.Error + " (" + e.AxMapError + ")", true);
#else
            MessageBoxMessage.Send("Error", "[" + (int)e.ErrorCode + "] " + e.Error, true);
#endif
            IsBusy = false;
        }


        private void MapControl_LayerChange(object sender, Map.Events.MapControl_LayerChange e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    UpdateMapLayers();
                    UpdateCommandsCanExecute();
                });
        }

        private void MapControl_EditingStateChange(object sender, Map.Events.MapControl_EditingStateChange e)
        {
            switch (e.EditingState)
            {
                case Map.Events.EditingState.None:
                    break;
                case Map.Events.EditingState.StartEditing:
                    break;
                case Map.Events.EditingState.AddShape:
                    IsAddingShape = true;
                    break;
                case Map.Events.EditingState.EditSshape:
                    IsEditingMap = true;
                    break;
                case Map.Events.EditingState.StopEditing:
                    IsAddingShape = false;
                    IsEditingMap = false;
                    break;
                case Map.Events.EditingState.SaveEditing:
                    break;
                case Map.Events.EditingState.DeleteShape:
                    if (e.EditingLayer is ResTBHazardMapLayer)
                    {
                        SelectedHazardMap = null;
                    }
                    if (e.EditingLayer is ResTBDamagePotentialLayer)
                    {
                        SelectedMappedObject = null;
                    }
                    //TODO: redraw not working...
                    //MapControl.Tools.Redraw(false); 
                    //MapControl.Tools.AxMap.Redraw();
                    break;
                default:
                    break;
            }


            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    UpdateCommandsCanExecute();
                });
        }

        private async void MapControl_Clicked(object sender, MapControl_Clicked e)
        {
            SelectedShapeIndex = e.ClickedShapeIndex;

            Type tyoe = e.GetClickedDBObject().GetType();

            if (e.GetClickedDBObject().GetType().BaseType == typeof(HazardMap))
            {
                var hazardMap = (HazardMap)e.GetClickedDBObject();

                using (ResTBContext db = new DB.ResTBContext())
                {
                    SelectedHazardMap = await db.HazardMaps
                        .Include(h => h.NatHazard)
                        .Include(h => h.Project)
                        .Where(m => m.ID == hazardMap.ID).FirstOrDefaultAsync();
                }
                if (SelectedHazardMap != null)
                {
                    SelectedHazardIndex = HazardIndexes.Where(h => h.Index == SelectedHazardMap.Index).SingleOrDefault();
                    SelectedNatHazard = SelectedHazardMap.NatHazard;
                }
            }

            if (e.GetClickedDBObject().GetType().BaseType == typeof(MappedObject))
            {
                var selectedObject = (MappedObject)e.GetClickedDBObject();

                if (IsCopyingResilience)
                {
                    if (selectedObject.ID != SelectedMappedObject.ID)
                    {
                        IsBusy = true;
//#if DEBUG
//                        MessageBoxMessage.Send($"Copy Resilience to {selectedObject.ID}", $"Resilience Values to copy: {SelectedMergedObjectParameter.ResilienceValuesMerged.Count}", true);
//#endif
                        await MappedObjectManager.CopyResilience(SelectedMappedObject.ID, selectedObject.ID);
                        MapControl.Tools.Redraw(true);

                        IsBusy = false;
                    }
                    else
                    {
#if DEBUG
                        MessageBoxMessage.Send($"Copy Resilience", $"Source selected", true);
#endif
                    }
                }
                else
                {
                    using (ResTBContext db = new DB.ResTBContext())
                    {
                        SelectedMappedObject = await db.MappedObjects.AsNoTracking()
                            .Include(m => m.Objectparameter.ObjectClass)
                            .Include(m => m.Objectparameter.HasProperties)
                            .Include(m => m.Objectparameter.MotherOtbjectparameter.HasProperties)
                            .Include(m => m.Objectparameter.MotherOtbjectparameter.ObjectClass)
                            .Include(pstate => pstate.Project.ProjectState)
                            .Include(m => m.Objectparameter.ObjectparameterPerProcesses)    //.Select(pp => pp.NatHazard))
                            .Include(m => m.Objectparameter.ResilienceFactors)
                            .Include(m => m.ResilienceValues)
                            .Include(m => m.FreeFillParameter)
                            .Include(m => m.Project)
                            .Where(m => m.ID == selectedObject.ID)
                            .FirstOrDefaultAsync();

                    }

                    MergeSelectedMappedObject();

                }
            }

            //TODO: Add more checks and casts!

            DispatcherHelper.CheckBeginInvokeOnUI(
                        () =>
                        {
                            UpdateCommandsCanExecute();
                        });

        }

        /// <summary>
        /// Merge object parameters (freefill, isstandard & motherobject)
        /// </summary>
        private void MergeSelectedMappedObject()
        {
            if (SelectedMappedObject == null)
            {
#if DEBUG
                throw new ArgumentNullException(nameof(SelectedMappedObject), "MergeSelectedMappedObject() not possible.");
#else
                return;
#endif
            }

            if (SelectedMappedObject.Objectparameter.IsStandard)
            {
                SelectedObjectClass = ObjectClasses.Where(c => c.ID == SelectedMappedObject.Objectparameter.ObjectClass.ID).SingleOrDefault();
                SelectedObjectParameter = ObjectParameters.Where(p => p.ID == SelectedMappedObject.Objectparameter.ID).SingleOrDefault();
            }
            else
            {
                SelectedObjectClass = ObjectClasses
                    .Where(c => c.ID == SelectedMappedObject.Objectparameter.MotherOtbjectparameter.ObjectClass.ID).SingleOrDefault();
                SelectedObjectParameter = ObjectParameters
                    .Where(p => p.ID == SelectedMappedObject.Objectparameter.MotherOtbjectparameter.ID).SingleOrDefault();
            }

            var tempObjParam = MappedObjectManager.GetMergedObjectparameter(SelectedMappedObject.ID);

            SelectedMergedPropertyDefinitions = new PropertyDefinitionCollection();

            var propDef = new PropertyDefinition();
            propDef.TargetProperties.Add(nameof(Objectparameter.Unity));
#if DEBUG
            propDef.TargetProperties.Add(nameof(Objectparameter.IsStandard));
            propDef.TargetProperties.Add(nameof(Objectparameter.ID));
#endif
            foreach (ObjectparameterHasProperties ohp in tempObjParam.HasProperties)
            {
                propDef.TargetProperties.Add(ohp.Property);
            }
            SelectedMergedPropertyDefinitions.Add(propDef);

            SelectedMergedObjectParameter = tempObjParam;
        }


        //TODO: Implement logic. Selected Object is missing...
        private void MapControl_SelectingStateChange(object sender, MapControl_SelectingStateChange e)
        {
            var selectedLayers = e.SelectingLayers;

            switch (e.SelectingState)
            {
                case SelectingState.None:
                    break;
                case SelectingState.StartSelecting:
                    break;
                case SelectingState.ShapeSelected:
                    break;
                case SelectingState.StopSelecting:
                    if (IsCopyingResilience)
                    {
                        SelectedMappedObject = null;
                        IsCopyingResilience = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private void MapControl_BusyStateChange(object sender, MapControl_BusyStateChange e)
        {
            string message = "";
#if DEBUG
            if (!string.IsNullOrEmpty(e.KeyOfSender))
                message += e.KeyOfSender;
            if (!string.IsNullOrEmpty(e.KeyOfSender) && !string.IsNullOrEmpty(e.Message))
                message += ": ";
#endif
            if (!string.IsNullOrEmpty(e.Message))
                message += e.Message;

            switch (e.BusyState)
            {
                case BusyState.Idle:
                    IsBusy = false;
                    IsBackgroundBusy = false;
                    StatusBarProgressBarVisible = false;
                    StatusBarMainString = message;
                    break;
                case BusyState.Busy:
                    IsBusy = true;
                    StatusBarMainString = message;
                    break;
                case BusyState.BackgroundBusy:
                    //IsBusy = false;
                    IsBackgroundBusy = true;
                    StatusBarMainString = string.Empty;
                    StatusBarProgressBarVisible = true;
                    StatusBarProgressBarMessageString = message;
                    //StatusBarProgressBarPercent = e.Percent;
                    break;
                default:
                    IsBusy = false;
                    throw new NotImplementedException();
                    //break;
            }

            StatusBarIndeterminate = false;
            StatusBarProgressBarPercent = e.Percent;

            DispatcherHelper.CheckBeginInvokeOnUI(
                 () =>
                 {
                     UpdateCommandsCanExecute();
                 });
        }

        #endregion


    }
}
