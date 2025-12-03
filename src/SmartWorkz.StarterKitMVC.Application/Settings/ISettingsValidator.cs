using SmartWorkz.StarterKitMVC.Domain.Settings;
using SmartWorkz.StarterKitMVC.Shared.Primitives;

namespace SmartWorkz.StarterKitMVC.Application.Settings;

public interface ISettingsValidator
{
    Result Validate(SettingDefinition definition, string? value);
}
