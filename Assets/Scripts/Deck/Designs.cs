using UnityEngine;

#region Damage

#region Melee
public class Stab : Design
{
    protected override void Init()
    {
        Title = "Stab";
        Category = DesignCategory.Melee;
        Color = Color.cyan;
        AddStat(St.Cost, 1);
        AddStat(St.UsesPerTurn, 1);
        AddStat(St.Damage, 6);
        
        base.Init();
    }

    protected override void OddLevelUp()
    {
        ModifyStat(St.Damage, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}
#endregion

#region Ranged
public class Slinger : Design
{
    protected override void Init()
    {
        Title = "Slinger";
        Category = DesignCategory.Ranged;
        Color = Color.red;
        AddStat(St.Cost, 2);
        AddStat(St.UsesPerTurn, 2);
        AddStat(St.Damage, 4);
        AddStat(St.MinRange, 1);
        AddStat(St.MaxRange, 1);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Damage, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}

public class Archer : Design
{
    protected override void Init()
    {
        Title = "Archer";
        Category = DesignCategory.Ranged;
        Color = Color.red;
        AddStat(St.Cost, 2);
        AddStat(St.UsesPerTurn, 2);
        AddStat(St.Damage, 3);
        AddStat(St.MinRange, 1);
        AddStat(St.MaxRange, 2);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Damage, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}

public class Marksman : Design
{
    protected override void Init()
    {
        Title = "Marksman";
        Category = DesignCategory.Ranged;
        Color = Color.red;
        AddStat(St.Cost, 3);
        AddStat(St.UsesPerTurn, 1);
        AddStat(St.Damage, 6);
        AddStat(St.MinRange, 3);
        AddStat(St.MaxRange, 3);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Damage, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}

#endregion

#region Area
public class Stomp : Design
{
    protected override void Init()
    {
        Title = "Stomp";
        Category = DesignCategory.Area;
        Color = Color.yellow;
        AddStat(St.Cost, 1);
        AddStat(St.UsesPerTurn, 1);
        AddStat(St.Damage, 2);
        AddStat(St.MaxRange, 1);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Damage, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}

public class Splatter : Design
{
    protected override void Init()
    {
        Title = "Splatter";
        Category = DesignCategory.Area;
        Color = Color.yellow;
        AddStat(St.Cost, 3);
        AddStat(St.UsesPerTurn, 1);
        AddStat(St.Damage, 1);
        AddStat(St.MaxRange, 2);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Damage, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}
#endregion


#endregion

#region Boost

public class Boost : Design
{
    protected override void Init()
    {
        Title = "Boost";
        Category = DesignCategory.Boost;
        Color = Color.magenta;
        AddStat(St.Cost, 1);
        AddStat(St.Boost, 2);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Boost, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.Boost, 1);
    }
}

#endregion

#region Movement Modifiers

public class Impede : Design
{
    protected override void Init()
    {
        Title = "Impede";
        Category = DesignCategory.Block;
        Color = new Color(0.54f, 0.54f, 0.67f);
        AddStat(St.Cost, 1);
        AddStat(St.UsesPerTurn, 1);
        AddStat(St.MinRange, 0);
        AddStat(St.MaxRange, 0);
        AddStat(St.Block, 1);
        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Block, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}

public class Reverse : Design
{
    protected override void Init()
    {
        Title = "Reverse";
        Category = DesignCategory.Reverse;
        Color = new Color(0f, 0.72f, 0.55f);
        Upgradeable = false;
        AddStat(St.Cost, 3);
        AddStat(St.UsesPerTurn, 1);

        base.Init();
    }

    protected override void OddLevelUp()
    {
        throw new System.NotImplementedException();
    }

    protected override void EvenLevelUp()
    {
        throw new System.NotImplementedException();
    }
}

public class Hop : Design
{
    protected override void Init()
    {
        Title = "Hop";
        Category = DesignCategory.Hop;
        Color = Color.green;
        AddStat(St.Cost, 1);
        AddStat(St.Hop, 1);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
    }

    protected override void EvenLevelUp()
    {
    }
}


#endregion

#region Debuffs

public class Poison : Design
{
    protected override void Init()
    {
        Title = "Poison";
        Category = DesignCategory.Poison;
        Color = new Color(0.41f, 0.67f, 0f);
        AddStat(St.Cost, 1);
        AddStat(St.UsesPerTurn, 1);
        AddStat(St.Poison, 3);

        base.Init();
    }
    
    protected override void OddLevelUp()
    {
        ModifyStat(St.Poison, 1);
    }

    protected override void EvenLevelUp()
    {
        ModifyStat(St.UsesPerTurn, 1);
    }
}

#endregion