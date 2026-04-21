namespace SmartWorkz.Core;
public enum EntityStatus
{
    [System.ComponentModel.DataAnnotations.Display(Name = "Active")]
    Active = 0,
    [System.ComponentModel.DataAnnotations.Display(Name = "Inactive")]
    Inactive = 1,
    [System.ComponentModel.DataAnnotations.Display(Name = "Archived")]
    Archived = 2,
    [System.ComponentModel.DataAnnotations.Display(Name = "Deleted")]
    Deleted = 3
}
