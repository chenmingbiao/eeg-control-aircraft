using System;
using libsvm;

/* Conversion notes (Andrew Poh):
 * Removed nested call of Streamreader constructor - original Java used BufferedReader
 * wrapped around FileReader.
 * 
 * Removed SupportClass because duplicated in svm library.
 * 
 * Replaced System.Console.arraycopy() with Array.Copy - this stemmed from a lousy
 * conversion of System.arraycopy().
 * 
 * Replaced System.Math.Max() with System.Math.Max().
 * 
 * Replaced (ArrayList)vx.addElement() in read_problem() with Add().
 */

class svm_train
{
	private svm_parameter param; // set by parse_command_line
	private svm_problem prob; // set by read_problem
	private svm_model model;
	private System.String input_file_name; // set by parse_command_line
	private System.String model_file_name; // set by parse_command_line
	private System.String error_msg;
	private int cross_validation = 0;
	private int nr_fold;
	
	private static void  exit_with_help()
	{
		System.Console.Out.Write("Usage: svm_train [options] training_set_file [model_file]\n" + "options:\n" + "-s svm_type : set type of SVM (default 0)\n" + "	0 -- C-SVC\n" + "	1 -- nu-SVC\n" + "	2 -- one-class SVM\n" + "	3 -- epsilon-SVR\n" + "	4 -- nu-SVR\n" + "-t kernel_type : set type of kernel function (default 2)\n" + "	0 -- linear: u'*v\n" + "	1 -- polynomial: (gamma*u'*v + coef0)^degree\n" + "	2 -- radial basis function: exp(-gamma*|u-v|^2)\n" + "	3 -- sigmoid: tanh(gamma*u'*v + coef0)\n" + "-d degree : set degree in kernel function (default 3)\n" + "-g gamma : set gamma in kernel function (default 1/k)\n" + "-r coef0 : set coef0 in kernel function (default 0)\n" + "-c cost : set the parameter C of C-SVC, epsilon-SVR, and nu-SVR (default 1)\n" + "-n nu : set the parameter nu of nu-SVC, one-class SVM, and nu-SVR (default 0.5)\n" + "-p epsilon : set the epsilon in loss function of epsilon-SVR (default 0.1)\n" + "-m cachesize : set cache memory size in MB (default 40)\n" + "-e epsilon : set tolerance of termination criterion (default 0.001)\n" + "-h shrinking: whether to use the shrinking heuristics, 0 or 1 (default 1)\n" + "-b probability_estimates: whether to train a SVC or SVR model for probability estimates, 0 or 1 (default 0)\n" + "-wi weight: set the parameter C of class i to weight*C, for C-SVC (default 1)\n" + "-v n: n-fold cross validation mode\n");
		System.Environment.Exit(1);
	}
	
	private void  do_cross_validation()
	{
		int i;
		int total_correct = 0;
		double total_error = 0;
		double sumv = 0, sumy = 0, sumvv = 0, sumyy = 0, sumvy = 0;
		double[] target = new double[prob.l];
		
		svm.svm_cross_validation(prob, param, nr_fold, target);
		if (param.svm_type == svm_parameter.EPSILON_SVR || param.svm_type == svm_parameter.NU_SVR)
		{
			for (i = 0; i < prob.l; i++)
			{
				double y = prob.y[i];
				double v = target[i];
				total_error += (v - y) * (v - y);
				sumv += v;
				sumy += y;
				sumvv += v * v;
				sumyy += y * y;
				sumvy += v * y;
			}
			System.Console.Out.Write("Cross Validation Mean squared error = " + total_error / prob.l + "\n");
			System.Console.Out.Write("Cross Validation Squared correlation coefficient = " + (((prob.l * sumvy - sumv * sumy) * (prob.l * sumvy - sumv * sumy)) / ((prob.l * sumvv - sumv * sumv) * (prob.l * sumyy - sumy * sumy))) + "\n");
		}
		else
			for (i = 0; i < prob.l; i++)
				if (target[i] == prob.y[i])
					++total_correct;
		System.Console.Out.Write("Cross Validation Accuracy = " + 100.0 * total_correct / prob.l + "%\n");
	}
	
	private void  run(System.String[] argv)
	{
		parse_command_line(argv);
		read_problem();
		error_msg = svm.svm_check_parameter(prob, param);
		
		if ((System.Object) error_msg != null)
		{
			System.Console.Error.Write("Error: " + error_msg + "\n");
			System.Environment.Exit(1);
		}
		
		if (cross_validation != 0)
		{
			do_cross_validation();
		}
		else
		{
			model = svm.svm_train(prob, param);
			svm.svm_save_model(model_file_name, model);
		}
	}
	
	
	private static double atof(System.String s)
	{
		return System.Double.Parse(s);
	}
	
	private static int atoi(System.String s)
	{
		return System.Int32.Parse(s);
	}
	
	private void  parse_command_line(System.String[] argv)
	{
		int i;
		
		param = new svm_parameter();
		// default values
		param.svm_type = svm_parameter.C_SVC;
		param.kernel_type = svm_parameter.RBF;
		param.degree = 3;
		param.gamma = 0; // 1/k
		param.coef0 = 0;
		param.nu = 0.5;
		param.cache_size = 40;
		param.C = 1;
		param.eps = 1e-3;
		param.p = 0.1;
		param.shrinking = 1;
		param.probability = 0;
		param.nr_weight = 0;
		param.weight_label = new int[0];
		param.weight = new double[0];
		
		// parse options
		for (i = 0; i < argv.Length; i++)
		{
			if (argv[i][0] != '-')
				break;
			++i;
			switch (argv[i - 1][1])
			{
				
				case 's': 
					param.svm_type = atoi(argv[i]);
					break;
				
				case 't': 
					param.kernel_type = atoi(argv[i]);
					break;
				
				case 'd': 
					param.degree = atof(argv[i]);
					break;
				
				case 'g': 
					param.gamma = atof(argv[i]);
					break;
				
				case 'r': 
					param.coef0 = atof(argv[i]);
					break;
				
				case 'n': 
					param.nu = atof(argv[i]);
					break;
				
				case 'm': 
					param.cache_size = atof(argv[i]);
					break;
				
				case 'c': 
					param.C = atof(argv[i]);
					break;
				
				case 'e': 
					param.eps = atof(argv[i]);
					break;
				
				case 'p': 
					param.p = atof(argv[i]);
					break;
				
				case 'h': 
					param.shrinking = atoi(argv[i]);
					break;
				
				case 'b': 
					param.probability = atoi(argv[i]);
					break;
				
				case 'v': 
					cross_validation = 1;
					nr_fold = atoi(argv[i]);
					if (nr_fold < 2)
					{
						System.Console.Error.Write("n-fold cross validation: n must >= 2\n");
						exit_with_help();
					}
					break;
				
				case 'w': 
					++param.nr_weight;
					{
						int[] old = param.weight_label;
						param.weight_label = new int[param.nr_weight];
						Array.Copy(old, 0, param.weight_label, 0, param.nr_weight - 1);
					}
					
					{
						double[] old = param.weight;
						param.weight = new double[param.nr_weight];
						Array.Copy(old, 0, param.weight, 0, param.nr_weight - 1);
					}
					
					param.weight_label[param.nr_weight - 1] = atoi(argv[i - 1].Substring(2));
					param.weight[param.nr_weight - 1] = atof(argv[i]);
					break;
				
				default: 
					System.Console.Error.Write("unknown option\n");
					exit_with_help();
					break;
				
			}
		}
		
		// determine filenames
		
		if (i >= argv.Length)
			exit_with_help();
		
		input_file_name = argv[i];
		
		if (i < argv.Length - 1)
			model_file_name = argv[i + 1];
		else
		{
			int p = argv[i].LastIndexOf((System.Char) '/');
			++p; // whew...
			model_file_name = argv[i].Substring(p) + ".model";
		}
	}
	
	// read in a problem (in svmlight format)
	
	private void  read_problem()
	{
		/* UPGRADE_TODO: Expected value of parameters of constructor
		 * 'java.io.BufferedReader.BufferedReader' are different in the equivalent in .NET.
		 * 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1092"'
		 */
		System.IO.StreamReader fp = new System.IO.StreamReader(input_file_name);
		System.Collections.ArrayList vy = new System.Collections.ArrayList(10);
		System.Collections.ArrayList vx = new System.Collections.ArrayList(10);
		int max_index = 0;
		
		while (true)
		{
			System.String line = fp.ReadLine();
			if ((System.Object) line == null)
				break;
			
			SupportClass.Tokenizer st = new SupportClass.Tokenizer(line, " \t\n\r\f:");
			
			vy.Add(st.NextToken());
			int m = st.Count / 2;
			svm_node[] x = new svm_node[m];
			for (int j = 0; j < m; j++)
			{
				x[j] = new svm_node();
				x[j].index = atoi(st.NextToken());
				x[j].value_Renamed = atof(st.NextToken());
			}
			if (m > 0)
				max_index = System.Math.Max(max_index, x[m - 1].index);
			vx.Add(x);
		}
		
		prob = new svm_problem();
		prob.l = vy.Count;
		prob.x = new svm_node[prob.l][];
		for (int i = 0; i < prob.l; i++)
			prob.x[i] = (svm_node[]) vx[i];
		prob.y = new double[prob.l];
		for (int i = 0; i < prob.l; i++)
			prob.y[i] = atof((System.String) vy[i]);
		
		if (param.gamma == 0)
			param.gamma = 1.0 / max_index;
		
		fp.Close();
	}
}