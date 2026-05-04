using System.Collections.Generic;

namespace NomadLogic.iBrand.Transformation.Filters
{
    public abstract class AbstractGreenScreenFilter : ImageFilter
    {
        abstract public void setParameters(Dictionary<string, float> parameters);
    }
}
