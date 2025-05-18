[Theory]
[InlineData(0)]
[InlineData(3)]
public void GenerateColor_AssignsCorrectHue(int idx)
{
    var vm = new HomePageViewModel();
    var color1 = vm.GenerateColor(idx);
    var color2 = vm.GenerateColor(idx);
    Assert.Equal(color1.Hue, color2.Hue);
}