using System;
using System.Collections.Generic;
using System.Globalization;
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
		private string Error = "Некорректные значения: ";


		// Конструктор
		public MathModel(string H, string W, string Vu, string Mu0, string N, string L, 
						string DeltaL, string B, string Tr, string Tu, string Alpha,
						string Ro, string C, string T0)
		{
			this.H = TryToParse(H, "\n Глубина");
			this.W = TryToParse(W, "\n Ширина");
			this.Vu = TryToParse(Vu, "\n Скорость крышки");
			this.Mu0 = TryToParse(Mu0, "\n Коэффициент консистенции");
			this.N = TryToParse(N, "\n Индекс течения материала");
			this.L = TryToParse(L, "\n Длина");
			this.DeltaL = TryToParse(DeltaL, "\n Шаг движения по длине канала");
			this.B = TryToParse(B, "\n Коэффициент вязкости");
			this.Tr = TryToParse(Tr, "\n Температура приведения");
			this.Tu = TryToParse(Tu, "\n Температура крышки");
			this.Alpha = TryToParse(Alpha, "\n Коэффициент теплоотдачи");
			this.Ro = TryToParse(Ro, "\n Плотность");
			this.C = TryToParse(C, "\n Удельная теплоемкость");
			this.T0 = TryToParse(T0, "\n Температура плавления");
		}

		public double TryToParse(string Var, string ErrorMsg)
		{
			double ResultVar;
			try
			{
				ResultVar = Double.Parse(Var, CultureInfo.InvariantCulture);
				if (ResultVar >= 0.001)
				{
					return ResultVar;
				}
				else
				{
					Error += ErrorMsg;
					return -1;
				}
			}
			catch
			{
				Error += ErrorMsg;
				return -1;
			}
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
				T[i] = Math.Round(MaterialTemperatureTI(Tr, B, K[i]), 2);
				Eta[i] = Math.Round(MaterialViscosityEtaI(Mu0, B, T[i], Tr, Gamma, N));
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
			return Math.Round(Tp, 2);
		}

		// Вязкость продукта
		public double GetEtaP()
		{
			return Math.Round(EtaP);
		}

		// Производительность
		public double GetG()
		{
			return Math.Round(G, 1);
		}
		public string GetError()
		{
			return Error;
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
			return Q * Ro * 3600;
		}

	}
}
