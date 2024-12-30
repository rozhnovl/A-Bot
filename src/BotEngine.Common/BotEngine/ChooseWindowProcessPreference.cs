namespace BotEngine;

public class ChooseWindowProcessPreference
{
	public string FilterMainModuleFileName;

	public string FilterMainModuleFilePath;

	public ChooseWindowProcessPreference()
	{
	}

	public ChooseWindowProcessPreference(string filterMainModuleFileName, string filterMainModuleFilePath = null)
	{
		FilterMainModuleFileName = filterMainModuleFileName;
		FilterMainModuleFilePath = filterMainModuleFilePath;
	}
}
