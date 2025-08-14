using System;

namespace Privileged.Components.Tests;

internal class TestModel
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public bool? IsOptional { get; set; }
    public string Option { get; set; } = "A";
    public string Description { get; set; } = "";
    public DateTime DateOfBirth { get; set; } = new DateTime(1990, 1, 1);
}
