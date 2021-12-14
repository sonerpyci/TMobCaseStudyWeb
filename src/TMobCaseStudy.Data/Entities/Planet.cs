namespace TMobCaseStudy.Data.Entities;

public class Planet
{
    public string Name { get; set; }
    public int HorizontalBoundary { get; set; }
    public int VerticalBoundary{ get; set; }

    public Planet(string name, int horizontalBoundary, int verticalBoundary)
    {
        Name = name;
        HorizontalBoundary = horizontalBoundary;
        VerticalBoundary = verticalBoundary;
    }

    public void Enlarge(int horizontalAdditionalSize, int verticalAdditionalSize)
    {
        HorizontalBoundary += horizontalAdditionalSize;
        VerticalBoundary += verticalAdditionalSize;
    }

    public void Resize(int horizontalBoundary, int verticalBoundary)
    {
        HorizontalBoundary = horizontalBoundary;
        VerticalBoundary = verticalBoundary;
    }
}