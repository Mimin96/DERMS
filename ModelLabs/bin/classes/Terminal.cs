//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FTN {
    using System;
    using FTN;
    
    
    /// An electrical connection point to a piece of conducting equipment. Terminals are connected at physical connection points called "connectivity nodes".
    public class Terminal : IdentifiedObject {
        
        /// ConductingEquipment has 1 or 2 terminals that may be connected to other ConductingEquipment terminals via ConnectivityNodes
        private ConductingEquipment cim_ConductingEquipment;
        
        private const bool isConductingEquipmentMandatory = true;
        
        private const string _ConductingEquipmentPrefix = "cim";
        
        /// Terminals interconnect with zero impedance at a node.  Measurements on a node apply to all of its terminals.
        private ConnectivityNode cim_ConnectivityNode;
        
        private const bool isConnectivityNodeMandatory = false;
        
        private const string _ConnectivityNodePrefix = "cim";
        
        public virtual ConductingEquipment ConductingEquipment {
            get {
                return this.cim_ConductingEquipment;
            }
            set {
                this.cim_ConductingEquipment = value;
            }
        }
        
        public virtual bool ConductingEquipmentHasValue {
            get {
                return this.cim_ConductingEquipment != null;
            }
        }
        
        public static bool IsConductingEquipmentMandatory {
            get {
                return isConductingEquipmentMandatory;
            }
        }
        
        public static string ConductingEquipmentPrefix {
            get {
                return _ConductingEquipmentPrefix;
            }
        }
        
        public virtual ConnectivityNode ConnectivityNode {
            get {
                return this.cim_ConnectivityNode;
            }
            set {
                this.cim_ConnectivityNode = value;
            }
        }
        
        public virtual bool ConnectivityNodeHasValue {
            get {
                return this.cim_ConnectivityNode != null;
            }
        }
        
        public static bool IsConnectivityNodeMandatory {
            get {
                return isConnectivityNodeMandatory;
            }
        }
        
        public static string ConnectivityNodePrefix {
            get {
                return _ConnectivityNodePrefix;
            }
        }
    }
}
