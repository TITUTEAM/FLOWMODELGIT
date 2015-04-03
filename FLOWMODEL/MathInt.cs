using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLOWMODEL
{
    interface MathInt
    {
		// Поправочный коэффициент F
		double CorrectionCoefficientF(double H, double W);
		// H -- ширина
		// W -- длинна
		
		// Объемный расход Q
		double VolumetricFlowRateQ(double H, double W, double Vu, double F);
		// H -- ширина
		// W -- длинна
		// Vu -- скорость верней крышки канала

		// Скорость деформации сдвига
		double ShearStainRateGamma(double H, double Vu);
		// H -- ширина
		// Vu -- скорость верней крышки канала
		
		// Удельный тепловой поток
		double HeatFluxQGamma(double H, double W, double Mu0, double N, double Gamma);
		// H -- ширина
		// W -- длинна
		// Mu0 -- коэффициент консистенции при температуре приведения
		// N -- индекс течения материала
		// Gamma -- скорость деформации сдвига
	}
}
