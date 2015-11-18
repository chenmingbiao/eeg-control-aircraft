using System;
namespace libsvm
{
	[Serializable]
	public class svm_problem
	{
		public int l;
		public double[] y;
		public svm_node[][] x;
	}
}