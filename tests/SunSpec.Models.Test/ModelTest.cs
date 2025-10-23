using Shouldly;

namespace SunSpec.Models.Test;

public class ModelTest
{
    [Fact]
    public void LoadModel()
    {
        Model model = Model.GetModel(701);
        model.ShouldNotBeNull();
        model.Group.Name.ShouldBe("DERMeasureAC");
        model.Group.Points.Count.ShouldBe(72);
    }
}