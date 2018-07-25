using System.Collections;
using System.Text;
using UnityEngine;

public class CommandLineModuleTime : CommandLineModule {

    private void Start()
    {
        commands.Add("timescale", TimeScale);
        commands.Add("slowmo", SlowMo);
        commands.Add("slomo", SlowMo);
        commands.Add("slowmotion", SlowMo);
        commands.Add("devicetime", DeviceTime);
        commands.Add("currenttimescale", CurrentTimeScale);
        commands.Add("currentts", CurrentTimeScale);
    }

    public override void Help()
    {
        StringBuilder helpMessage = new StringBuilder();

        helpMessage.Append("timescale float:amount\n");
        helpMessage.Append("slowmo float:amount float:duration\n");
        helpMessage.Append("devicetime\n");
        helpMessage.Append("currenttimescale");

        CommandLineCore.Print(helpMessage.ToString());
    }

    private void TimeScale(string[] args)
    {
        string amount = args[2];
        Time.timeScale = float.Parse(amount);
        CommandLineCore.Print("TimeScale set to " + Time.timeScale);
    }

    private void SlowMo(string[] args)
    {
        string amount = args[2];
        string duration = args[3];
        StartCoroutine(ActivateSlowMo(float.Parse(amount), float.Parse(duration)));
    }

    IEnumerator ActivateSlowMo(float amount, float duration)
    {
        float previousTimeScale = Time.timeScale;
        Time.timeScale = amount;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = previousTimeScale;
    }

    private void DeviceTime(string[] args)
    {
        CommandLineCore.Print(System.DateTime.Now.ToString());
    }

    private void CurrentTimeScale(string[] args)
    {
        CommandLineCore.Print("Current TimeScale: " + Time.timeScale);
    }
}
