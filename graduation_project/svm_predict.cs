using System;
using libsvm;

/* Conversion notes (Andrew Poh):
 * Support Class removed.
 * BinaryWriter.Write() replaced with Write().
 * Removed nested construction of StreamReader.
 */

class svm_predict
{
	private static double atof(System.String s)
	{
		return System.Double.Parse(s);
	}
	
	private static int atoi(System.String s)
	{
		return System.Int32.Parse(s);
	}
	
	private static void  predict(System.IO.StreamReader input, System.IO.BinaryWriter output, svm_model model, int predict_probability)
	{
		int correct = 0;
		int total = 0;
		double error = 0;
		double sumv = 0, sumy = 0, sumvv = 0, sumyy = 0, sumvy = 0;
		
		int svm_type = svm.svm_get_svm_type(model);
		int nr_class = svm.svm_get_nr_class(model);
		int[] labels = new int[nr_class];
		double[] prob_estimates = null;
		
		if (predict_probability == 1)
		{
			if (svm_type == svm_parameter.EPSILON_SVR || svm_type == svm_parameter.NU_SVR)
			{
				System.Console.Out.Write("Prob. model for test data: target value = predicted value + z,\nz: Laplace distribution e^(-|z|/sigma)/(2sigma),sigma=" + svm.svm_get_svr_probability(model) + "\n");
			}
			else
			{
				svm.svm_get_labels(model, labels);
				prob_estimates = new double[nr_class];
				//UPGRADE_ISSUE: Method 'java.io.DataOutputStream.Write' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaioDataOutputStreamWrite_javalangString"'
				output.Write("labels");
				for (int j = 0; j < nr_class; j++)
				{
					//UPGRADE_ISSUE: Method 'java.io.DataOutputStream.Write' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaioDataOutputStreamWrite_javalangString"'
					output.Write(" " + labels[j]);
				}
				//UPGRADE_ISSUE: Method 'java.io.DataOutputStream.Write' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaioDataOutputStreamWrite_javalangString"'
				output.Write("\n");
			}
		}
		while (true)
		{
			System.String line = input.ReadLine();
			if ((System.Object) line == null)
				break;
			
			SupportClass.Tokenizer st = new SupportClass.Tokenizer(line, " \t\n\r\f:");
			
			double target = atof(st.NextToken());
			int m = st.Count / 2;
			svm_node[] x = new svm_node[m];
			for (int j = 0; j < m; j++)
			{
				x[j] = new svm_node();
				x[j].index = atoi(st.NextToken());
				x[j].value_Renamed = atof(st.NextToken());
			}
			
			double v;
			if (predict_probability == 1 && (svm_type == svm_parameter.C_SVC || svm_type == svm_parameter.NU_SVC))
			{
				v = svm.svm_predict_probability(model, x, prob_estimates);
				//UPGRADE_ISSUE: Method 'java.io.DataOutputStream.Write' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaioDataOutputStreamWrite_javalangString"'
				output.Write(v + " ");
				for (int j = 0; j < nr_class; j++)
				{
					//UPGRADE_ISSUE: Method 'java.io.DataOutputStream.Write' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaioDataOutputStreamWrite_javalangString"'
					output.Write(prob_estimates[j] + " ");
				}
				//UPGRADE_ISSUE: Method 'java.io.DataOutputStream.Write' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaioDataOutputStreamWrite_javalangString"'
				output.Write("\n");
			}
			else
			{
				v = svm.svm_predict(model, x);
				//UPGRADE_ISSUE: Method 'java.io.DataOutputStream.Write' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaioDataOutputStreamWrite_javalangString"'
				output.Write(v + "\n");
			}
			
			if (v == target)
				++correct;
			error += (v - target) * (v - target);
			sumv += v;
			sumy += target;
			sumvv += v * v;
			sumyy += target * target;
			sumvy += v * target;
			++total;
		}
		System.Console.Out.Write("Accuracy = " + (double) correct / total * 100 + "% (" + correct + "/" + total + ") (classification)\n");
		System.Console.Out.Write("Mean squared error = " + error / total + " (regression)\n");
		System.Console.Out.Write("Squared correlation coefficient = " + ((total * sumvy - sumv * sumy) * (total * sumvy - sumv * sumy)) / ((total * sumvv - sumv * sumv) * (total * sumyy - sumy * sumy)) + " (regression)\n");
	}
	
	private static void  exit_with_help()
	{
		System.Console.Error.Write("usage: svm_predict [options] test_file model_file output_file\n" + "options:\n" + "-b probability_estimates: whether to predict probability estimates, 0 or 1 (default 0); one-class SVM not supported yet\n");
		System.Environment.Exit(1);
	}
	
	[STAThread]
	public static void  Main1(System.String[] argv)
	{
		int i, predict_probability = 0;
		
		// parse options
		for (i = 0; i < argv.Length; i++)
		{
			if (argv[i][0] != '-')
				break;
			++i;
			switch (argv[i - 1][1])
			{
				
				case 'b': 
					predict_probability = atoi(argv[i]);
					break;
				
				default: 
					System.Console.Error.Write("unknown option\n");
					exit_with_help();
					break;
				
			}
		}
		if (i >= argv.Length)
			exit_with_help();
		try
		{
			//UPGRADE_TODO: Expected value of parameters of constructor 'java.io.BufferedReader.BufferedReader' are different in the equivalent in .NET. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1092"'
			System.IO.StreamReader input = new System.IO.StreamReader(argv[i]); // Input file
			System.IO.BinaryWriter output = new System.IO.BinaryWriter(new System.IO.FileStream(argv[i + 2], System.IO.FileMode.Create)); // Output file
			svm_model model = svm.svm_load_model(argv[i + 1]); // Model file
			predict(input, output, model, predict_probability);
		}
		catch (System.IO.FileNotFoundException e)
		{
			exit_with_help();
		}
		catch (System.IndexOutOfRangeException e)
		{
			exit_with_help();
		}
	}
}