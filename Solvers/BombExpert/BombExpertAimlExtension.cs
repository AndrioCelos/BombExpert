using System;
using System.Linq;
using Aiml;

namespace BombExpert;
public class BombExpertAimlExtension : IAimlExtension {
	public void Initialise() {
		foreach (var type in typeof(BombExpertAimlExtension).Assembly.GetExportedTypes().Where(t => !t.IsInterface && typeof(IModuleSolver).IsAssignableFrom(t))) {
			AimlLoader.AddCustomSraixService((IModuleSolver) Activator.CreateInstance(type)!);
		}
	}
}
