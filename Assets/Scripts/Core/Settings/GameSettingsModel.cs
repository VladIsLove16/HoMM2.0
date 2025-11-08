namespace Adventure.Settings.Model
{
    public class GameSettingsModel
    {
        public AudioSettingsModel Audio = new();
        public GraphicsSettingsModel Graphics = new();
        public ControlSettingsModel Controls = new();
    }
}
