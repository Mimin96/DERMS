using System;

namespace FTN.Common
{	

    public enum UnitSymbol : short
    {        
        A = 0,
		deg = 1,
		degC = 2,
		F = 3,
		g = 4,
		h = 5,
		H = 6,
		Hz = 7,
		J = 8,
		m = 9,
		m2 = 10,
		m3 = 11,
		min = 12,
		N = 13,
		ohm = 14,
		Pa = 15,
		rad = 16,
		s = 17,
        S = 18,
        V = 19,
        VA = 20,
        Vah = 21,
        Var = 22,
        VArh = 23,
        W = 24,
        Wh = 25,
        Unknown = 26
    }

    public enum UnitMultiplier : short
    {
        c = 0,
        d = 1,
        G = 2,
        k = 3,
        m = 4,
        M = 5,
        micro = 6,
        n = 7,                
        p = 8,
        T = 9,                
        none = 10
    }

    public enum CurveStyle : short
    {        
		CONST = 0,
		FORMULA = 1,
		RAMP = 2,
        STRAIGHT = 3,
        None = 4
    }

    public enum MeasurementType : short
    {
        ActivePower = 0,
        ReactivePower = 1,
        Voltage = 2,
        Discrete = 3,
        None = 4
    }

    public enum ConductorType : short
    {
        BusSegment = 0,
        Cable = 1,
        OhCable = 1,
        Overhead = 2,
        None = 3
    }

    public enum EnergySourceType : short
    {
        Distribution = 0,
        Interconnection = 1,
        None = 3
    }

    public enum GeneratorType : short
    {
        Wind = 0,
        Solar = 1,
        Battery = 2,
        None = 3
    }
}
