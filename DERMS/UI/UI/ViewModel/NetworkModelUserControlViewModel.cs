using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using UI.Model;
using UI.Resources;
using MaterialDesignThemes.Wpf;
using DERMSCommon.UIModel;
using DERMSCommon.UIModel.ThreeViewModel;
using DERMSCommon;

namespace UI.ViewModel
{
    public class NetworkModelUserControlViewModel : BindableBase
    {
        #region TreeView Data adn Commands
        private RelayCommand<long> _networkModelCommand;
        private RelayCommand<long> _geographicalRegionCommand;
        private RelayCommand<long> _geographicalSubRegionCommand;
        private RelayCommand<long> _substationCommand;
        private RelayCommand<long> _substationElementCommand;
        public List<NetworkModelTreeClass> _networkModel;

        public List<NetworkModelTreeClass> NetworkModel
        {
            get
            {
                return _networkModel;
            }
            set
            {
                _networkModel = value;
                OnPropertyChanged("NetworkModel");
            }
        }
        public ICommand NetworkModelCommand
        {
            get
            {
                if (_networkModelCommand == null)
                {
                    _networkModelCommand = new RelayCommand<long>(NetworkModelCommandExecute);
                }

                return _networkModelCommand;
            }
        }
        public ICommand GeographicalRegionCommand
        {
            get
            {
                if (_geographicalRegionCommand == null)
                {
                    _geographicalRegionCommand = new RelayCommand<long>(GeographicalRegionCommandExecute);
                }

                return _geographicalRegionCommand;
            }
        }
        public ICommand GeographicalSubRegionCommand
        {
            get
            {
                if (_geographicalSubRegionCommand == null)
                {
                    _geographicalSubRegionCommand = new RelayCommand<long>(GeographicalSubRegionCommandExecute);
                }

                return _geographicalSubRegionCommand;
            }
        }
        public ICommand SubstationCommand
        {
            get
            {
                if (_substationCommand == null)
                {
                    _substationCommand = new RelayCommand<long>(SubstationCommandExecute);
                }

                return _substationCommand;
            }
        }
        public ICommand SubstationElementCommand
        {
            get
            {
                if (_substationElementCommand == null)
                {
                    _substationElementCommand = new RelayCommand<long>(SubstationElementCommandExecute);
                }

                return _substationElementCommand;
            }
        }
        #endregion

        public NetworkModelUserControlViewModel()
        {
            NetworkModelItems = new ObservableCollection<NetworkModelViewClass>() { new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosacsssssssssssssssssssssdsfsdfsdfsssss", "Info 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35"),
                                                                                    new NetworkModelViewClass(Brushes.Green, PackIconKind.About, "Potrosacccccccccc", "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35 dfffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"),
                                                                                    new NetworkModelViewClass(Brushes.Yellow, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Green, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Green, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Green, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info"),
                                                                                    new NetworkModelViewClass(Brushes.Red, PackIconKind.About, "Potrosac", "Info")};
        }

        #region Properties
        public TreeNode<NodeData> Tree { get; set; }
        public ObservableCollection<NetworkModelViewClass> NetworkModelItems { get; set; }
        #endregion

        #region TreeView Commands Execute
        public void NetworkModelCommandExecute(long gid)
        {
            Console.Beep();
        }
        public void GeographicalRegionCommandExecute(long gid)
        {
            Console.Beep();
        }
        public void GeographicalSubRegionCommandExecute(long gid)
        {
            Console.Beep();
        }
        public void SubstationCommandExecute(long gid)
        {
            Console.Beep();
        }
        public void SubstationElementCommandExecute(long gid)
        {
            Console.Beep();
        }
        #endregion
    }
}
