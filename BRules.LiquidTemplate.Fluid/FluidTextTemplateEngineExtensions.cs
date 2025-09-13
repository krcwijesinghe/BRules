using Fluid;

namespace BRules;

public static class FluidTextTemplateEngineExtensions
{
    public static IRulesEngineBuilder UseFluidTextTemplateEngine(this IRulesEngineBuilder self, FluidParser? parser = null)
    {
        return self.UseTextTemplateEngine(new FluidTextTemplateEngine(parser));
    }

    public static IRulesEngineBuilder UseFluidTextTemplateEngine(this IRulesEngineBuilder self, Action<FluidParser> phaserConfigAction)
    {
        var parser = new FluidParser();
        phaserConfigAction(parser);
        return self.UseTextTemplateEngine(new FluidTextTemplateEngine(parser));
    }
}
