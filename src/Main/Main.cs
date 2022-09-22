using Godot;
using System.Linq;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Main : Control
{
    public static Main Instance;
    private Menu _menu;
    private Game _game;
    
    private RichTextLabel _dialogueLabel;
    private System.Random _random = new System.Random();
    private NodePool<AudioStreamPlayer> _noisePool;
    private DialogueSequence _currentSequence;
    private bool _inPage;
    private int _textIndex;
    private int _pageIndex;

    private DialoguePage CurrentPage => _currentSequence.Pages[_pageIndex];
    private DialogueMood CurrentMood => _currentSequence.Character.Moods[CurrentPage.Mood];
    
    [Export] private string _busName;
    [Export] private PackedScene _audioPlayer;
    [Export] private Dictionary<string, DialogueSequence> _sequences;
    [Signal] public delegate void FinishedBook();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _menu = GetNode<Menu>("%Menu");
        _menu.Connect(nameof(Menu.PlayPressed), this, nameof(OnPlayPressed));
        _menu.Connect(nameof(Menu.VolumeSliderChanged), this, nameof(OnVolumeChanged));

        _game = GetNode<Game>("%Game");

        _dialogueLabel = GetNode<RichTextLabel>("%DialogueText");
        
        var maxPoolSize = 1;
        foreach (var kvp in _sequences)
        {
            var maxTime = kvp.Value.Character.Moods.Values.SelectMany(v => v.Sounds).Max(s => s.GetLength() * 3);
            var maxPageRatio = kvp.Value.Pages.Max(p => p.Speed / p.NoisePeriod);
            var poolSize = Mathf.CeilToInt(maxTime * maxPageRatio) + 1;
            if (poolSize > maxPoolSize)
                maxPoolSize = poolSize;
        }
        _noisePool = new NodePool<AudioStreamPlayer>(this, maxPoolSize, i =>
        {
            var p = _audioPlayer.Instance<AudioStreamPlayer>();
            p.Name += i;
            return p;
        });
    }

    public async void OnPlayPressed()
    {
        _menu.SetProcessInput(false);
        await this.AwaitNextProcess();

        var linearVolume = GD.Db2Linear(AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(_busName)));
        
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_menu, "modulate", Colors.Transparent, 1f).SetTrans(Tween.TransitionType.Linear);
        //tween.Parallel().TweenMethod(this, nameof(OnVolumeChanged), linearVolume, 0f, 1f).SetTrans(Tween.TransitionType.Linear);
        tween.TweenCallback(this, "remove_child", new Array {_menu});
        await ToSignal(tween, "finished");
        
        _game.StartGame();
    }

    public void OnVolumeChanged(float value)
    {
        var idx = AudioServer.GetBusIndex(_busName);
        if (idx == -1) return;
        AudioServer.SetBusVolumeDb(idx, GD.Linear2Db(value));
    }
    
    public void SetupDialogue(string sequenceName)
    {
        _currentSequence = _sequences[sequenceName];
        _inPage = false;
        _textIndex = 0;
        _pageIndex = 0;
        _dialogueLabel.Text = "";
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
            if (_pageIndex < _currentSequence.Pages.Length)
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
