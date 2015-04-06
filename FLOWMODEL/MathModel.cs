using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FLOWMODEL
{
	public class MathModel
	{
		private double H, W, Vu, Mu0, N, L, DeltaL, B, Tr, Tu, Alpha, Ro, C, T0;
		private double F, Q, Gamma, qGamma, qAlpha, Li, G, Tp, EtaP;
		private int m;
		private double[] K, T, Eta;


		// Конструктор
		public MathModel(double H, double W, double Vu, double Mu0, double N, double L, 
						double DeltaL, double B, double Tr, double Tu, double Alpha,
						double Ro, double C, double T0)
		{
			this.H = H;
			this.W = W;
			this.Vu = Vu;
			this.Mu0 = Mu0;
			this.N = N;
			this.L = L;
			this.DeltaL = DeltaL;
			this.B = B;
			this.Tr = Tr;
			this.Tu = Tu;
			this.Alpha = Alpha;
			this.Ro = Ro;
			this.C = C;
			this.T0 = T0;
		}

		// Проверка
		public String Check()
		{
			String VarName = "Некорректные значения: ";
			if (H <= 0.001)	{ VarName += "H, "; }
			if (W <= 0.001) { VarName += "W, "; }
			if (Vu <= 0.001) { VarName += "Vu, "; }
			if (Mu0 <= 0.001) { VarName += "Mu0, "; }
			if (N <= 0.001) { VarName += "n, "; }
			if (L <= 0.001) { VarName += "L, "; }
			if (DeltaL <= 0.001) { VarName += "deltaL, "; }
			if (B <= 0.001) { VarName += "b, "; }
			if (Tr <= 0.001) { VarName += "Tr, "; }
			if (Tu <= 0.001) { VarName += "Tu, "; }
			if (Alpha <= 0.001) { VarName += "alpha, "; }
			if (Ro <= 0.001) { VarName += "ro, "; }
			if (C <= 0.001) { VarName += "C, "; }
			if (T0 <= 0.001) { VarName += "T0, "; }

			return VarName;
		}

		// Алгоритм
		public void Algorithm()
		{
			F = CorrectionCoefficientF(H, W);
			Q = VolumetricFlowRateQ(H, W, Vu, F);
			Gamma = ShearStainRateGamma(H, Vu);
			qGamma = HeatFluxQGamma(H, W, Mu0, N, Gamma);
			qAlpha = HeatFluxQAlpha(H, Tu, B, Tr, Alpha);
			m = NumberOfStepsM(L, DeltaL);

			K = new double[m];
			T = new double[m];
			Eta = new double[m];

			for (int i = 0; i < m; i++)
			{
				Li = i * DeltaL;
				K[i] = HeatBalanceKI(B, qGamma, W, Alpha, qAlpha, Li, Ro, C, Q, T0, Tr);
				T[i] = MaterialTemperatureTI(Tr, B, K[i]);
				Eta[i] = MaterialViscosityEtaI(Mu0, B, T[i], Tr, Gamma, N);
			}

			Tp = T[m - 1];
			EtaP = Eta[m - 1];

			G = ChannelOutputG(Q, Ro);
		}

		// Распределение температуры по длинне канала
		public double[] GetTI()
		{
			return T;
		}

		// Распределение вязкости по длинне канала
		public double[] GetEtaI()
		{
			return Eta;
		}
		
		// Температура продукта
		public double GetTp()
		{
			return Tp;
		}

		// Вязкость продукта
		public double GetEtaP()
		{
			return EtaP;
		}

		// Производительность
		public double GetG()
		{
			return G;
		}
		
		// Поправочный коэффициент F
		// H -- ширина
		// W -- длинна
		private double CorrectionCoefficientF(double H, double W)
		{
			return 0.125 * System.Math.Pow((H / W), 2) - 0.625 * (H / W) + 1;
		}

		// Объемный расход Q
		// H -- ширина
		// W -- длинна
		// Vu -- скорость верней крышки канала
		// F -- поравочный коэффициент -- CorrectionCoefficientF
		private double VolumetricFlowRateQ(double H, double W, double Vu, double F)
		{
			return ((W * H * Vu) / 2) * F;
		}

		// Скорость деформации сдвига
		// H -- ширина
		// Vu -- скорость верней крышки канала
		private double ShearStainRateGamma(double H, double Vu)
		{
			return Vu / H;
		}

		// Удельный тепловой поток qGamma
		// H -- ширина
		// W -- длинна
		// Mu0 -- коэффициент консистенции при температуре приведения
		// N -- индекс течения материала
		// Gamma -- скорость деформации сдвига -- ShearStainRateGamma 
		private double HeatFluxQGamma(double H, double W, double Mu0, double N, double Gamma)
		{
			return W * H * Mu0 * System.Math.Pow(Gamma, N + 1);
		}


		// Удельный тепловой поток qAlpha
		// W -- длинна
		// Tu -- температура верхней крышки
		// B -- темепературный коэффициент вязкости
		// Tr -- температура приведения
		// Alpha -- коэффициент теплоотдачи??????
		private double HeatFluxQAlpha(double W, double Tu, double B, double Tr, double Alpha)
		{
			return W * ((Alpha / B) - Alpha * Tu + Alpha * Tr);
		}


		// Число шагов вычисления модели m
		// L -- длинна канала
		// DeltaL -- длинна промежутка?????
		private int NumberOfStepsM(double L, double DeltaL)
		{
			double m = L / DeltaL;
			if (m - System.Math.Round(m) < 0.5)
			{
				return Convert.ToInt32(System.Math.Round(m)) + 1;
			}
			else
			{
				return Convert.ToInt32(System.Math.Round(m));
			}
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
		private double HeatBalanceKI(double B, double QGamma, double W, double Alpha, double QAlpha,
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
		private double MaterialTemperatureTI(double Tr, double B, double Ki)
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
		private double MaterialViscosityEtaI(double Mu0, double B, double Ti, double Tr, double Gamma, double N)
		{
			return Mu0 * System.Math.Exp(-B * (Ti - Tr)) * System.Math.Pow(Gamma, N - 1);
		}

		// Прозводительность канала
		// Ro -- плотность
		// Q -- удельный расход -- VolumetricFlowRateQ
		private double ChannelOutputG(double Q, double Ro)
		{
			return Q * Ro;
		}



	}
}
