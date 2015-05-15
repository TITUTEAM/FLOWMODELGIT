using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Word;

namespace FLOWMODEL
{
	class Report
	{
		private double[] T;
		private double[] Eta;
		private Application WordApp;
		private object missing;
		private String FileName;
		public Report(String FileName, MathModel Model, String MatherialName)
		{
			WordApp = new Application();
			missing = System.Reflection.Missing.Value;
			Document WordDoc = WordApp.Documents.Add(ref missing, ref missing, ref missing, ref missing);
			this.FileName = FileName;

			Generation(WordDoc, Model, MatherialName);

			WordDoc.SaveAs2(FileName);

			WordApp.Quit(ref missing, ref missing, ref missing);
			WordApp = null;
		}

		private void Generation(Document WordDoc, MathModel Model, String MatherialName)
		{
			DateTime dt = DateTime.Now;
			foreach(Section section in WordDoc.Sections)
			{
				Range headerRange = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
				headerRange.Fields.Add(headerRange, WdFieldType.wdFieldPage);
				headerRange.ParagraphFormat.Alignment =	WdParagraphAlignment.wdAlignParagraphCenter;
				headerRange.Font.ColorIndex = WdColorIndex.wdGray25;
				headerRange.Font.Size = 10;
				headerRange.Text = App.Current.Resources["Header"] + " " + dt.ToString();
			}

			Paragraph para1 = WordDoc.Content.Paragraphs.Add(ref missing);

			para1.Range.Font.Size = 18;
			para1.Range.Font.Bold = 1;
			para1.Range.Text = "\t" + App.Current.Resources["InitialData"].ToString();
			para1.Range.InsertParagraphAfter();

			Table table = WordDoc.Tables.Add(para1.Range, 3, 2, ref missing, ref missing);

			table.Rows[1].Cells[1].Range.Text = App.Current.Resources["ModelGeomParams"]
							+ "\n" + App.Current.Resources["ModelGeomParamsWidth"] + " " + Model.W.ToString()
							+ "\n" + App.Current.Resources["ModelGeomParamsDepth"] + " " + Model.H.ToString()
							+ "\n" + App.Current.Resources["ModelGeomParamsLength"] + " " + Model.L.ToString();
			table.Rows[1].Cells[1].Range.Font.Name = "Times New Roman";
			table.Rows[1].Cells[1].Range.Font.Size = 14;

			table.Rows[1].Cells[2].Range.Text = App.Current.Resources["ModelProcessParams"]
							+ "\n" + App.Current.Resources["ModelProcessParamsSpeed"] + " " + Model.Vu.ToString()
							+ "\n" + App.Current.Resources["ModelProcessParamsCoverTemp"] + " " + Model.Tu.ToString() + "\n"
							+ "\n" + App.Current.Resources["ModelProcessSolutionParams"]
							+ "\n" + App.Current.Resources["ModelProcessParamsSpeed"] + " " + Model.DeltaL.ToString();
			table.Rows[1].Cells[2].Range.Font.Name = "Times New Roman";
			table.Rows[1].Cells[2].Range.Font.Size = 14;

			table.Rows[3].Cells[1].Range.Text = App.Current.Resources["ModelMaterialParams"]
							+ "\n" + App.Current.Resources["ModelMaterialName"] + " " + MatherialName
							+ "\n" + App.Current.Resources["ModelMaterialDensity"] + " " + Model.Ro.ToString()
							+ "\n" + App.Current.Resources["ModelMaterialHeatCapacity"] + " " + Model.C.ToString()
							+ "\n" + App.Current.Resources["ModelMaterialTemperatureMelting"] + " " + Model.T0.ToString();
			table.Rows[3].Cells[1].Range.Font.Name = "Times New Roman";
			table.Rows[3].Cells[1].Range.Font.Size = 14;

			table.Rows[3].Cells[2].Range.Text = App.Current.Resources["ModelEmpiricalCoef"]
							+ "\n" + App.Current.Resources["ModelConsistenceCoef"] + " " + Model.Mu0.ToString()
							+ "\n" + App.Current.Resources["ModelTemperatureCoef"] + " " + Model.B.ToString()
							+ "\n" + App.Current.Resources["ModelTemperatureReduction"] + " " + Model.Tr.ToString()
							+ "\n" + App.Current.Resources["ModelFluidIndex"] + " " + Model.N.ToString()
							+ "\n" + App.Current.Resources["ModelHeatIrradiance"] + " " + Model.Alpha.ToString();
			table.Rows[3].Cells[2].Range.Font.Name = "Times New Roman";
			table.Rows[3].Cells[2].Range.Font.Size = 14;

			para1.Range.InsertParagraphAfter();

			Paragraph para2 = WordDoc.Content.Paragraphs.Add(ref missing);

			para2.Range.Font.Size = 18;
			para2.Range.Font.Bold = 1;
			para2.Range.Text = "\t" + App.Current.Resources["Output"].ToString();
			para2.Range.InsertParagraphAfter();
			
			T = Model.GetTI();
			Eta = Model.GetEtaI();

			Table OutputTable = WordDoc.Tables.Add(para2.Range, T.Length + 1, 3, ref missing, ref missing);
			OutputTable.Borders.Enable = 1;

			OutputTable.Rows[1].Cells[1].Range.Text = App.Current.Resources["TableLength"].ToString();
			OutputTable.Rows[1].Cells[1].Range.Font.Name = "Times New Roman";
			OutputTable.Rows[1].Cells[2].Range.Text = App.Current.Resources["TableTemperature"].ToString();
			OutputTable.Rows[1].Cells[2].Range.Font.Name = "Times New Roman";
			OutputTable.Rows[1].Cells[3].Range.Text = App.Current.Resources["TableViscosity"].ToString();
			OutputTable.Rows[1].Cells[3].Range.Font.Name = "Times New Roman";

			for (int i = 1; i < OutputTable.Rows.Count; i++)
			{
				OutputTable.Rows[i + 1].Cells[1].Range.Text = (Convert.ToDouble(i - 1) / 10).ToString();
				OutputTable.Rows[i + 1].Cells[1].Range.Font.Name = "Times New Roman";
				OutputTable.Rows[i + 1].Cells[2].Range.Text = T[i - 1].ToString();
				OutputTable.Rows[i + 1].Cells[2].Range.Font.Name = "Times New Roman";
				OutputTable.Rows[i + 1].Cells[3].Range.Text = Eta[i - 1].ToString();
				OutputTable.Rows[i + 1].Cells[3].Range.Font.Name = "Times New Roman";
			}

			para2.Range.InsertParagraphAfter();

			Paragraph para3 = WordDoc.Content.Paragraphs.Add(ref missing);
			para3.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;

			Table PicTable = WordDoc.Tables.Add(para3.Range, 4, 1, ref missing, ref missing);
			PicTable.Rows[1].Cells[1].Range.InlineShapes.AddPicture(FileName + "T.png", ref missing, ref missing, ref missing);

			PicTable.Rows[2].Cells[1].Range.Text = App.Current.Resources["TempPict"].ToString();
			PicTable.Rows[2].Cells[1].Range.Font.Name = "Times New Roman";
			PicTable.Rows[2].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
			PicTable.Rows[2].Cells[1].Range.Font.Size = 14;

			PicTable.Rows[3].Cells[1].Range.InlineShapes.AddPicture(FileName + "V.png", ref missing, ref missing, ref missing);

			PicTable.Rows[4].Cells[1].Range.Text = App.Current.Resources["ViscPict"].ToString();
			PicTable.Rows[4].Cells[1].Range.Font.Name = "Times New Roman";
			PicTable.Rows[4].Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
			PicTable.Rows[4].Cells[1].Range.Font.Size = 14;



		}
	}
}
