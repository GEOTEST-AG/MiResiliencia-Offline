using ResTB.Map.Layer;
using ResTB.Translation.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace ResTB.GUI.Model.Layers
{
    public class LayersModel : INotifyPropertyChanged
    {
        #region Data

        bool? _isChecked = false;
        LayersModel _parent;
        public ILayer Layer { get; set; }
        private string _name = "";
        public string Name
        {
            get
            {
                if (_name != "") return _name;
                return Layer.Name;
            }
            set { _name = value; }
        }

        #endregion // Data

        #region CreateFoos


        public LayersModel()
        {

            this.Children = new ObservableCollection<LayersModel>();
        }

        public LayersModel(string name)
        {
            this.Name = name;
            this.Children = new ObservableCollection<LayersModel>();
        }

        public LayersModel(string name, ILayer layer)
        {
            this.Name = name;
            this.Layer = layer;
            this.Children = new ObservableCollection<LayersModel>();
            //this.SetIsManuallyChecked(false, true, true);
            this.SetIsChecked(layer.IsVisible, true, true);
        }

        public LayersModel(ILayer layer)
        {
            this.Layer = layer;
            this.Children = new ObservableCollection<LayersModel>();
            //this.SetIsManuallyChecked(false, true, true);
            this.SetIsChecked(layer.IsVisible, true, true);
        }

        public void Initialize(LayersModel selectedLayer)
        {
            foreach (LayersModel child in this.Children)
            {
                child._parent = this;

                if (selectedLayer?.Layer?.Name == child.Layer?.Name) child.IsInitiallySelected = true;

                child.Initialize(selectedLayer);
            }
        }

        public void MakeLayerVisible(Map.MapControlTools mapControlTools)
        {
            if (Layer != null) mapControlTools.SetLayerVisible(Layer.Name, (bool)IsChecked);
            foreach (var ch in this.Children)
            {
                ch.MakeLayerVisible(mapControlTools);
            }

        }

        #endregion // CreateFoos

        #region Properties

        public ObservableCollection<LayersModel> Children { get; set; }

        public bool IsInitiallySelected { get; set; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get
            {
                if (Layer != null)
                {
                    return Layer.IsVisible;
                }
                else return _isChecked;

            }
            set
            {

                if (_manuallyChecked == true)
                {
                    this.SetIsChecked(true, true, true);
                }
                else
                {
                    this.SetIsChecked(value, true, true);
                }
            }

        }

        public bool? _manuallyChecked;

        public bool? IsManuallyChecked
        {
            get
            {
                if (_manuallyChecked == true) return true;
                return IsChecked;
                //else return IsChecked;
            }
            set
            {
                if (value == true) _manuallyChecked = true;
                else _manuallyChecked = value;
                SetIsManuallyChecked(value, true, true);
                if (value == true) IsChecked = true;
                else IsChecked = value;
            }

        }

        public List<LayersModel> getAllChildren(List<LayersModel> currentList)
        {
            if (currentList == null)
                return new List<LayersModel>();

            foreach (LayersModel lm in Children)
            {
                lm.getAllChildren(currentList);
            }
            if (currentList.Where(m => m.Name == this.Name).Count() == 0) currentList.Add(this);
            else
            {
                if (currentList.Where(m => m.Layer?.Name == this.Layer?.Name).Count() == 0) currentList.Add(this);

            }
            return currentList;
        }

        public void PutAllChildrenToManually(ILayer layer)
        {
            foreach (LayersModel lm in Children)
            {
                lm.PutAllChildrenToManually(layer);
            }
            if (Layer == layer)
            {
                this._manuallyChecked = true;
                this.IsManuallyChecked = true;
                //VerifyManuallyCheckState();
            }
            return;
        }


        private LayersModel ToOverJumpLayer = null;

        public LayersModel OnWhichLayerIsNotFirstChildren(LayersModel me)
        {
            if (_parent == null) return null;
            if (me.Layer?.Name == me._parent.Children[0].Layer?.Name) return OnWhichLayerIsNotFirstChildren(me._parent);
            if (me.Name == me._parent.Children[0].Name) return OnWhichLayerIsNotFirstChildren(me._parent);

            foreach (LayersModel lm in me._parent.Children)
            {
                if (lm == me) break;
                ToOverJumpLayer = lm;
            }
            return me._parent;
        }


        public List<LayersModel> getLayersToMoveUp(out int howMuchStepsUp)
        {
            howMuchStepsUp = 1;
            // check if this layer is top of layergroup
            if (this.Layer != null)
            {
                if (_parent?.Children.First().Layer?.Name == this.Layer?.Name)
                {
                    LayersModel nextJumpingParentLayer = OnWhichLayerIsNotFirstChildren(_parent);
                    int nextOverjumpLayerPosition = ToOverJumpLayer.Layer.LayerPosition;
                    List<LayersModel> allChildren = _parent.getAllChildren(new List<LayersModel>());
                    howMuchStepsUp = nextOverjumpLayerPosition + allChildren.Where(m => m.Layer != null).Count() - Layer.LayerPosition;

                    return allChildren;
                }
            }
            // Node -> Move all Children inside Node
            if (_parent?.Children.First().Name == this.Name)
            {
                LayersModel nextJumpingParentLayer = OnWhichLayerIsNotFirstChildren(_parent);
                int nextOverjumpLayerPosition = ToOverJumpLayer.Layer.LayerPosition;

                List<LayersModel> allChildren = _parent.getAllChildren(new List<LayersModel>());
                LayersModel firstLayerLayer = allChildren.Where(m => m.Layer != null).FirstOrDefault();
                if (firstLayerLayer != null) howMuchStepsUp = nextOverjumpLayerPosition + allChildren.Where(m => m.Layer != null).Count() - firstLayerLayer.Layer.LayerPosition;

                return allChildren;
            }
            // Layer, but there is a layer or node before, just move me up
            if (this.Layer != null) return new List<LayersModel>() { this };
            // Node, but there is a layer or node before, just move all my children up
            LayersModel nextJumpingParentLayer2 = OnWhichLayerIsNotFirstChildren(_parent);
            int nextOverjumpLayerPosition2 = ToOverJumpLayer.Layer.LayerPosition;
            List<LayersModel> allChildren2 = _parent.getAllChildren(new List<LayersModel>());
            LayersModel firstLayerLayer2 = allChildren2.Where(m => m.Layer != null).FirstOrDefault();
            if (firstLayerLayer2 != null) howMuchStepsUp = nextOverjumpLayerPosition2 + allChildren2.Where(m => m.Layer != null).Count() - firstLayerLayer2.Layer.LayerPosition;


            return this.getAllChildren(new List<LayersModel>());
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            //if (value == _isChecked)
            //   return;

            if (Layer != null) Layer.IsVisible = (bool)value;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                foreach (var ch in this.Children)
                {
                    ch.SetIsChecked(_isChecked, true, false);
                }

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged("IsManuallyChecked");
        }

        void SetIsManuallyChecked(bool? value, bool updateChildren, bool updateParent)
        {
            //if (value == _manuallyChecked)
            //    return;

            _manuallyChecked = value;

            if (updateChildren && _manuallyChecked.HasValue)
                foreach (var ch in this.Children)
                {
                    ch.SetIsManuallyChecked(_manuallyChecked, true, false);
                }

            if (updateParent && _parent != null)
                _parent.VerifyManuallyCheckState();

            this.OnPropertyChanged("IsManuallyChecked");
        }


        public void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        public void VerifyManuallyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i]._manuallyChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsManuallyChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion // Properties


        public static LayersModel CreateLayersModel(List<ILayer> layers, LayersModel selectedLayer, ObservableCollection<LayersModel> oldModel = null)
        {

            LayersModel allLayers = new LayersModel(Resources.Layers);
            LayersModel projects = new LayersModel(Resources.Project_Layers);
            LayersModel rasters = new LayersModel(Resources.Rasters);
            LayersModel hazardMaps = new LayersModel(Resources.Hazard_Map);
            LayersModel resilience = new LayersModel(Resources.Resilience);
            LayersModel riskmap = new LayersModel(Resources.RiskMap);

            ObservableCollection<ILayer> orderedLayers = new ObservableCollection<ILayer>();
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.CustomLayerRaster)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.Perimeter)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.HazardMapBefore)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.HazardMapAfter)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.DamagePotential)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.ResilienceBefore)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.ResilienceAfter)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.MitigationMeasure)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.RiskMap)) orderedLayers.Add(customlayer);
            foreach (var customlayer in layers.Where(m => m.LayerType == LayerType.ProjectLayer).Where(m => ((ResTBPostGISLayer)m).ResTBPostGISType == ResTBPostGISType.RiskMapAfter)) orderedLayers.Add(customlayer);



            foreach (var item in orderedLayers)
            {
                if (item.LayerType == LayerType.CustomLayerRaster) rasters.Children.Add(new LayersModel(item));
                if (item.LayerType == LayerType.ProjectLayer)
                {
                    if ((item.GetType() == typeof(ResTBResilienceLayer)) && (item.ShapeCount > 0))
                    {
                        ResTBResilienceLayer resLayer = (ResTBResilienceLayer)item;
                        if (projects.Children?.Where(m => m.Name == Resources.Resilience).Count() == 0) projects.Children.Add(resilience);
                        resilience.Children.Add(new LayersModel(item));
                        resilience.VerifyCheckState();
                    }
                    else if ((item.GetType() == typeof(ResTBRiskMapLayer)) && (item.ShapeCount > 0))
                    {
                        ResTBRiskMapLayer rmLayer = (ResTBRiskMapLayer)item;
                        if (projects.Children?.Where(m => m.Name == Resources.RiskMap).Count() == 0) projects.Children.Add(riskmap);
                        riskmap.Children.Add(new LayersModel(item));
                        riskmap.VerifyCheckState();
                    }
                    else if (item.GetType() == typeof(ResTBHazardMapLayer))
                    {
                        ResTBHazardMapLayer hazardMapLayer = (ResTBHazardMapLayer)item;
                        if (projects.Children?.Where(m => m.Name == Resources.Hazard_Map).Count() == 0) projects.Children.Add(hazardMaps);


                        // Add or create Before Mitigation or After Mitigation LayerGroup
                        LayersModel beforeOrAfter = new LayersModel();
                        if (hazardMapLayer.ResTBPostGISType == ResTBPostGISType.HazardMapBefore) beforeOrAfter = hazardMaps.Children.Where(m => m.Name == Resources.Before_Mitigation).FirstOrDefault();
                        else if (hazardMapLayer.ResTBPostGISType == ResTBPostGISType.HazardMapAfter) beforeOrAfter = hazardMaps.Children.Where(m => m.Name == Resources.After_Mitigation).FirstOrDefault();
                        if (beforeOrAfter == null)
                        {
                            if (hazardMapLayer.ResTBPostGISType == ResTBPostGISType.HazardMapBefore) beforeOrAfter = new LayersModel(Resources.Before_Mitigation);
                            else if (hazardMapLayer.ResTBPostGISType == ResTBPostGISType.HazardMapAfter) beforeOrAfter = new LayersModel(Resources.After_Mitigation);
                            hazardMaps.Children.Add(beforeOrAfter);
                        }

                        // Add or create NatHazard Group
                        LayersModel natHazardGroup = beforeOrAfter.Children.Where(m => m.Name == hazardMapLayer.NatHazard.ToString()).FirstOrDefault();
                        if (natHazardGroup == null)
                        {
                            natHazardGroup = new LayersModel(hazardMapLayer.NatHazard.ToString());
                            beforeOrAfter.Children.Add(natHazardGroup);
                        }
                        natHazardGroup.Children.Add(new LayersModel(hazardMapLayer.Index.ToString(), hazardMapLayer));
                        natHazardGroup.VerifyCheckState();
                        beforeOrAfter.VerifyCheckState();
                        hazardMaps.VerifyCheckState();

                    }
                    else
                    {
                        if ((item.GetType() == typeof(ResTBPerimeterLayer)) || (item.ShapeCount > 0)) projects.Children.Add(new LayersModel(item));
                    }

                }

            }

            if ((projects.Children != null) && (projects.Children.Count > 0))
            {
                //projects.VerifyManuallyCheckState();
                projects.VerifyCheckState();
                if (allLayers.Children?.Count() > 0) allLayers.Children.Add(projects);
                else allLayers.Children = new ObservableCollection<LayersModel>() { projects };
            }
            if ((rasters.Children != null) && (rasters.Children.Count > 0))
            {
                rasters.VerifyCheckState();
                if (allLayers.Children?.Count() > 0) allLayers.Children.Add(rasters);
                else allLayers.Children = new ObservableCollection<LayersModel>() { rasters };
            }
            allLayers.Initialize(selectedLayer);
            allLayers.VerifyCheckState();

            if ((oldModel != null) && (oldModel.Count > 0))
            {
                var layerList = oldModel.First()?.getAllChildren(new List<LayersModel>()).Where(m => m._manuallyChecked == true).ToList();
                //var newLayers = allLayers.getAllChildren(new List<LayersModel>());
                foreach (LayersModel lmOld in layerList)
                {
                    allLayers.PutAllChildrenToManually(lmOld.Layer);

                }


            }


            return allLayers;
        }

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
