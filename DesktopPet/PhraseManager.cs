namespace DesktopPet;

/// <summary>
/// 可爱短句管理器：随机返回短句
/// </summary>
public static class PhraseManager
{
    private static readonly string[] Phrases =
    [
        "摸我干嘛~",
        "今天也要加油哦!",
        "别打扰我摸鱼~",
        "喵~",
        "你来啦!",
        "人家在忙呢...",
        "摸摸头~",
        "好困啊...",
        "嘿嘿，被你发现了~",
        "我才没有在偷懒!",
        "一起摸鱼吧~",
        "今天天气真好呢!",
    ];

    private static readonly Random Rng = new();

    public static string GetRandom() => Phrases[Rng.Next(Phrases.Length)];
}
