namespace SmartWorkz.Core;
public enum ResultStatus
{
    [System.ComponentModel.DataAnnotations.Display(Name = "Success")]
    Success = 0,
    [System.ComponentModel.DataAnnotations.Display(Name = "Failure")]
    Failure = 1,
    [System.ComponentModel.DataAnnotations.Display(Name = "Validation Error")]
    ValidationError = 2,
    [System.ComponentModel.DataAnnotations.Display(Name = "Not Found")]
    NotFound = 3,
    [System.ComponentModel.DataAnnotations.Display(Name = "Unauthorized")]
    Unauthorized = 4,
    [System.ComponentModel.DataAnnotations.Display(Name = "Forbidden")]
    Forbidden = 5
}
