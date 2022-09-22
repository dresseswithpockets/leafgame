using Godot;
using System.Linq;
using Godot.Collections;

public class Dialogue : Spatial
{
    [Export] public DialogueCharacter Character;
    [Export] public DialoguePage[] Pages;
    [Export] public PackedScene AudioPlayer;
    [Export] public bool AutoPlay;
    [Export] public bool Billboard;

    public DialoguePage CurrentPage => Pages[_pageIndex];
    public DialogueMood CurrentMood => Character.Moods[CurrentPage.Mood];

    private RichTextLabel _dialogueLabel;
    private bool _inPage;
    private int _pageIndex;
    private int _textIndex;
    private System.Random _random = new System.Random();
    private NodePool<AudioStreamPlayer> _noisePool;

    [Signal]
    public delegate void FinishedBook();

    public override void _Ready()
    {
        _dialogueLabel = GetNode<RichTextLabel>("Viewport/DialogueControl/RichTextLabel");
        var maxTime = Character.Moods.Values.SelectMany(v => v.Sounds).Max(s => s.GetLength() * 3);
        var maxPageRatio = Pages.Max(p => p.Speed / p.NoisePeriod);
        var poolSize = Mathf.CeilToInt(maxTime * maxPageRatio) + 1;
        _noisePool = new NodePool<AudioStreamPlayer>(this, poolSize, i =>
        {
            var p = AudioPlayer.Instance<AudioStreamPlayer>();
            p.Name += i;
            return p;
        });
        if (AutoPlay)
            StartPage();
    }

    public override void _Process(float delta)
    {
        if (Billboard)
        {
            var cam = GetNode<Camera>("/root/Main/UIOverlay ViewportContainer/Viewport/UIOverlay Camera");
            var newTransform = GlobalTransform;
            newTransform.basis = new Basis(cam.GlobalTransform.basis.RotationQuat());
            GlobalTransform = newTransform;
        }
    }

    public async void StartPage()
    {
        _inPage = true;
        var noiseCounter = 1f;
        for (_textIndex = 0; _textIndex < CurrentPage.Text.Length; _textIndex++)
        {
            _dialogueLabel.Text = CurrentPage.Text.Substring(0, _textIndex + 1);
            await this.AwaitTimer(1f/CurrentPage.Speed);
            var lastChar = _dialogueLabel.Text.Last();
            if (!char.IsLetterOrDigit(lastChar)) continue;

            if (noiseCounter >= 1f)
            {
                var player = _noisePool.GetNode();
                player.Stream = CurrentMood.Sounds[_random.Next(0, CurrentMood.Sounds.Count)];
                player.Connect("finished", this, nameof(OnNoisePlayerFinished), new Array { player });
                player.Play();
                
                var randomPitch = (float)(_random.NextDouble() * (CurrentMood.PitchHigh - CurrentMood.PitchLow)) +
                                  CurrentMood.PitchLow;
                player.PitchScale = randomPitch;
                noiseCounter -= 1f;
            }

            noiseCounter += 1f / CurrentPage.NoisePeriod;
        }
        _inPage = false;
        _textIndex = 0;
    }

    public DialoguePage ContinuePage()
    {
        if (_inPage)
        {
            _textIndex = CurrentPage.Text.Length;
            _dialogueLabel.Text = CurrentPage.Text;
        }
        else
        {
            _pageIndex++;
            if (_pageIndex < Pages.Length)
                StartPage();
            else
            {
                EmitSignal(nameof(FinishedBook));
                _dialogueLabel.Text = "";
                return null;
            }
        }

        return CurrentPage;
    }

    public void ResetAndStartPage()
    {
        _pageIndex = 0;
        StartPage();
    }

    public void OnNoisePlayerFinished(AudioStreamPlayer player)
    {
        _noisePool.Release(player);
        player.Disconnect("finished", this, nameof(OnNoisePlayerFinished));
    }
}
