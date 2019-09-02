using System;
using System.Collections.Generic;
using System.Text;
using Aiml;

namespace BombExpert {
	public interface IModuleSolver : ISraixService {
		public void GenerateAiml(string path, int ruleSeed);
	}
}
