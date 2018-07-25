using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommandLineModuleScene : CommandLineModule {

    private void Start()
    {
        commands.Add("loadscene", LoadScene);
        commands.Add("reloadscene", ReloadScene);
        commands.Add("loadnextscene", LoadNextScene);
        commands.Add("loadprevoiusscene", LoadPreviousScene);
        commands.Add("sceneindex", SceneIndex);
    }

    public override void Help()
    {
        StringBuilder helpMessage = new StringBuilder();

        helpMessage.Append("loadscene int:sceneIndex\n");
        helpMessage.Append("reloadscene\n");
        helpMessage.Append("loadnextscene\n");
        helpMessage.Append("loadpreviousscene\n");
        helpMessage.Append("sceneindex");

        CommandLineCore.Print(helpMessage.ToString());
    }

    public void ReloadScene(string[] args)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string[] args)
    {
        try
        {
            int sceneToLoad = int.Parse(args[2]);
            SceneManager.LoadScene(sceneToLoad);
        }
        catch (System.Exception)
        {
            SceneManager.LoadScene(args[2]);
        }
    }

    public void LoadNextScene(string[] args)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadPreviousScene(string[] args)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void SceneIndex(string[] args)
    {
        CommandLineCore.Print("Current scene index: " + SceneManager.GetActiveScene().buildIndex);
    }
}
