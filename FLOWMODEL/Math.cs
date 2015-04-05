using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FLOWMODEL
{
	public static class Math
	{
		// Поправочный коэффициент F
		// H -- ширина
		// W -- длинна
		double CorrectionCoefficientF(double H, double W)
		{
			return 0.125 * System.Math.Pow((H / W), 2) - 0.625 * (H / W) + 1;
		}

		// Объемный расход Q
		// H -- ширина
		// W -- длинна
		// Vu -- скорость верней крышки канала
		// F -- поравочный коэффициент -- CorrectionCoefficientF
		double VolumetricFlowRateQ(double H, double W, double Vu, double F)
		{
			return ((W * H * Vu) / 2) * F;
		}

		// Скорость деформации сдвига
		// H -- ширина
		// Vu -- скорость верней крышки канала
		double ShearStainRateGamma(double H, double Vu)
		{
			return Vu / H;
		}


		// Удельный тепловой поток qGamma
		// H -- ширина
		// W -- длинна
		// Mu0 -- коэффициент консистенции при температуре приведения
		// N -- индекс течения материала
		// Gamma -- скорость деформации сдвига -- ShearStainRateGamma 
		double HeatFluxQGamma(double H, double W, double Mu0, double N, double Gamma)
		{
			return W * H * Mu0 * System.Math.Pow(Gamma, N + 1);
		}


		// Удельный тепловой поток qAlpha
		// W -- длинна
		// Tu -- температура верхней крышки
		// B -- темепературный коэффициент вязкости
		// Tr -- температура приведения
		// Alpha -- коэффициент теплоотдачи??????
		double HeatFluxQAlpha(double W, double Tu, double B, double Tr, double Alpha)
		{
			return W * ((Alpha / B) - Alpha * Tu + Alpha * Tr);
		}


		// Число шагов вычисления модели m
		// L -- длинна канала
		// DeltaL -- длинна промежутка?????
		double NumberOfStepsM(double L, double DeltaL)
		{
			return L / DeltaL;
		}


		// Уравнение теплового баланса Кi
		// При вызове функции надо проверить, чтобы 0 < L -- текущая координата по длинне канала < L -- Длинна канала
		// B -- темепературный коэффициент вязкости
		// QGamma -- удельный тепловой поток qGamma
		// W -- длинна
		// Alpha -- коэффициент теплоотдачи??????
		// QAlpha -- удельный тепловой поток qAlpha
		// L -- длинна канала
		// Ro -- плотность
		// С -- удельная теплоемкость
		// Q -- удельный расход -- VolumetricFlowRateQ
		// T0 -- начальная температура
		// Tr -- температура приведения
		double HeatBalanceKI(double B, double QGamma, double W, double Alpha, double QAlpha,
							double L, double Ro, double C, double Q, double T0, double Tr)
		{
			return ((B * QGamma + W * Alpha) / (B * QAlpha)) * 
				(1 - System.Math.Exp(-((B * QAlpha * L) / (Ro * C * Q)))) +
				System.Math.Exp(B * (T0 - Tr - ((QAlpha * L) / (Ro * C * Q))));
		}

		// Температура материала в iтом сечении
		// Tr -- температура приведения
		// B -- темепературный коэффициент вязкости
		// Ki -- уравнение теплового баланса HeatBalanceKI
		double MaterialTemperatureTI(double Tr, double B, double Ki)
		{
			return Tr + ((1 / B) * System.Math.Log(Ki));
		}

		// Вязкость матеряла в iтом сечении
		// Mu0 -- коэффициент консистенции при температуре приведения
		// B -- темепературный коэффициент вязкости
		// Ti -- температура материала в iтом сечении
		// Tr -- температура приведения
		// Gamma -- скорость деформации сдвига -- ShearStainRateGamma 
		// N -- индекс течения материала
		double MaterialViscosityEtaI(double Mu0, double B, double Ti, double Tr, double Gamma, double N)
		{
			return Mu0 * System.Math.Exp(-B * (Ti - Tr)) * System.Math.Pow(Gamma, N - 1);
		}

		// Прозводительность канала
		// Ro -- плотность
		// Q -- удельный расход -- VolumetricFlowRateQ
		double ChannelOutputG(double Q, double Ro)
		{
			return Q * Ro;
		}



	}
}
