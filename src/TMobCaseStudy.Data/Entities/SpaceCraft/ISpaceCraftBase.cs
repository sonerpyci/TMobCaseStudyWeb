using TMobCaseStudy.Data.Definitions;

namespace TMobCaseStudy.Data.Entities.SpaceCraft;

public interface ISpaceCraftBase
{
    public void Rotate(Rotation rotation);
    public void Move();
}