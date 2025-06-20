using Godot;
using OpenNova.Definition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OpenNova.DefinitionsParsers
{
    public class ItemDefParser
    {
        public List<ItemDefinition> Parse(byte[] data)
        {
            string content = System.Text.Encoding.UTF8.GetString(data);
            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return ParseLines(lines);
        }

        private List<ItemDefinition> ParseLines(string[] lines)
        {
            List<ItemDefinition> items = new List<ItemDefinition>();
            ItemDefinition currentItem = null;

            int lineNumber = 0;
            GD.Print($"Parsing item definitions file with {lines.Length} lines");

            foreach (string rawLine in lines)
            {
                lineNumber++;
                string line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                {
                    continue;
                }

                int commentIndex = line.IndexOf("//");
                if (commentIndex >= 0)
                {
                    line = line.Substring(0, commentIndex).Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                }

                if (line.StartsWith("begin "))
                {
                    string itemName = ExtractQuotedString(line.Substring(6));
                    currentItem = new ItemDefinition();
                    currentItem.Init();
                }
                else if (line.Equals("end"))
                {
                    if (currentItem != null)
                    {
                        if (currentItem.InternalId == 0 && currentItem.Id > 0)
                        {
                            currentItem.InternalId = currentItem.Id - 100000;
                            if (currentItem.InternalId < 0)
                                currentItem.InternalId = currentItem.Id;
                        }

                        items.Add(currentItem);
                        currentItem = null;
                    }
                    else
                    {
                        GD.PrintErr("Found 'end' without matching 'begin' block");
                    }
                }
                // Process key-value pairs
                else if (currentItem != null)
                {
                    // Split on first whitespace
                    string[] parts = line.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        string key = parts[0].Trim().ToLower(); // Normalize key to lowercase
                        string value = parts[1].Trim();

                        switch (key)
                        {
                            case "id":
                                if (int.TryParse(value, out int id))
                                {
                                    currentItem.Id = id;
                                    // Set internal ID (used in game)
                                    currentItem.InternalId = id - 100000;
                                    if (currentItem.InternalId < 0)
                                        currentItem.InternalId = id;
                                }
                                break;
                            case "type":
                                if (Enum.TryParse<EntityType>(value, true, out var entityType))
                                {
                                    currentItem.EntityType = entityType;
                                }
                                break;
                            case "subtype":
                                currentItem.SubType = value;
                                break;
                            case "graphic":
                                var gfxPath = FileUtils.ResolveResourcePath(
                                    FileUtils.EnsureExtension(value, "3di")
                                );
                                if (!string.IsNullOrEmpty(gfxPath))
                                {
                                    currentItem.Graphic = ResourceLoader.Load<PackedScene>(gfxPath);
                                }
                                break;
                            case "graphicenemy":
                                currentItem.GraphicEnemy = value;
                                break;
                            case "text_id":
                                currentItem.TextId = value;
                                break;
                            case "unit_type":
                                if (byte.TryParse(value, out byte unitType))
                                {
                                    currentItem.UnitType = unitType;
                                }
                                break;
                            case "hp":
                                if (float.TryParse(value, out float hp))
                                {
                                    currentItem.Hp = hp;
                                }
                                break;
                            case "armor":
                                if (float.TryParse(value, out float armor))
                                {
                                    currentItem.Armor = armor;
                                }
                                break;
                            case "criticalhp":
                                if (float.TryParse(value, out float critHp))
                                {
                                    currentItem.CriticalHp = critHp;
                                }
                                break;
                            case "criticaldrain":
                                if (float.TryParse(value, out float critDrain))
                                {
                                    currentItem.CriticalDrain = critDrain;
                                }
                                break;
                            case "radarsig":
                                if (float.TryParse(value, out float radarSig))
                                {
                                    currentItem.RadarSig = radarSig;
                                }
                                break;
                            case "heatsig":
                                if (float.TryParse(value, out float heatSig))
                                {
                                    currentItem.HeatSig = heatSig;
                                }
                                break;
                            case "damagereduction":
                                if (float.TryParse(value, out float damageReduction))
                                {
                                    currentItem.DamageReduction = damageReduction;
                                }
                                break;
                            case "damagereductionscale":
                                if (float.TryParse(value, out float damageReductionScale))
                                {
                                    currentItem.DamageReductionScale = damageReductionScale;
                                }
                                break;
                            case "kz":
                                if (float.TryParse(value, out float kz))
                                {
                                    currentItem.Kz = kz;
                                }
                                break;
                            case "score":
                                if (float.TryParse(value, out float score))
                                {
                                    currentItem.Score = score;
                                }
                                break;
                            case "scale":
                                if (float.TryParse(value, out float scale))
                                {
                                    currentItem.Scale = scale;
                                }
                                break;
                            case "animdef":
                            case "anim_def":
                                currentItem.AnimDef = value;
                                break;
                            case "virtualdisplay":
                                currentItem.VirtualDisplay = value;
                                break;
                            case "hudimage":
                                currentItem.HudImage = value;
                                break;
                            case "shadowtexture":
                                currentItem.ShadowTexture = value;
                                break;
                            case "shadowwidth":
                                if (float.TryParse(value, out float shadowWidth))
                                {
                                    currentItem.ShadowWidth = shadowWidth;
                                }
                                break;
                            case "shadowheight":
                                if (float.TryParse(value, out float shadowHeight))
                                {
                                    currentItem.ShadowHeight = shadowHeight;
                                }
                                break;
                            case "shadowoffsetx":
                                if (float.TryParse(value, out float shadowOffsetX))
                                {
                                    currentItem.ShadowOffsetX = shadowOffsetX;
                                }
                                break;
                            case "shadowoffsety":
                                if (float.TryParse(value, out float shadowOffsetY))
                                {
                                    currentItem.ShadowOffsetY = shadowOffsetY;
                                }
                                break;
                            case "huskshadow":
                                currentItem.HuskShadow = value;
                                break;
                            case "husk":
                                currentItem.Husk = value;
                                break;
                            case "huskfinal":
                                currentItem.HuskFinal = value;
                                break;
                            case "huskswapatsec":
                                if (float.TryParse(value, out float huskSwapAtSec))
                                {
                                    currentItem.HuskSwapAtSec = huskSwapAtSec;
                                }
                                break;
                            case "huskswapat":
                                if (float.TryParse(value, out float huskSwapAt))
                                {
                                    currentItem.HuskSwapAt = huskSwapAt;
                                }
                                break;
                            case "husksubparts":
                                if (byte.TryParse(value, out byte huskSubParts))
                                {
                                    currentItem.HuskSubParts = huskSubParts;
                                }
                                break;
                            case "husksubparttype":
                                var huskParts = value.Split(' ');
                                if (huskParts.Length >= 2 &&
                                    int.TryParse(huskParts[0], out int partIndex) &&
                                    byte.TryParse(huskParts[1], out byte partType))
                                {
                                    currentItem.HuskSubPartTypes[partIndex] = partType;
                                }
                                break;
                            case "aifunction":
                            case "ai_function":
                                currentItem.AiFunction = value;
                                break;
                            case "renderfunction":
                            case "render_function":
                                currentItem.RenderFunction = value;
                                break;
                            case "movefunction":
                            case "move_function":
                                currentItem.MoveFunction = value;
                                break;
                            case "diskfunction":
                            case "disk_function":
                                currentItem.DiskFunction = value;
                                break;
                            case "inputfunction":
                            case "input_function":
                                currentItem.InputFunction = value;
                                break;
                            case "default_aip":
                                currentItem.DefaultAip = value;
                                break;

                            // Death time
                            case "deathtime":
                                if (int.TryParse(value, out int deathTime))
                                {
                                    currentItem.DeathTime = deathTime;
                                }
                                break;
                            case "phraseset":
                            case "phrase_set":
                                if (int.TryParse(value, out int phraseSet))
                                {
                                    currentItem.PhraseSet = phraseSet;
                                }
                                break;
                            case "sid":
                                if (int.TryParse(value, out int sid))
                                {
                                    currentItem.SID = sid;
                                }
                                break;
                            case "powerupdef":
                                currentItem.PowerUpDef = value;
                                break;
                            case "clipsize":
                                if (int.TryParse(value, out int clipSize))
                                {
                                    currentItem.ClipSize = clipSize;
                                }
                                break;
                            case "ammocloseattack":
                            case "ammo_closeattack":
                                currentItem.AmmoCloseAttack = value;
                                break;
                            case "ammoeasyrocket":
                            case "ammo_easyrocket":
                                currentItem.AmmoEasyRocket = value;
                                break;
                            case "ammoadvancedrocket":
                            case "ammo_advancedrocket":
                                currentItem.AmmoAdvancedRocket = value;
                                break;
                            case "ammomarker3":
                            case "ammo_marker3":
                                currentItem.AmmoMarker3 = value;
                                break;
                            case "launchupscloseattack":
                            case "launchups_closeattack":
                                currentItem.LaunchUpsCloseAttack = value;
                                break;
                            case "launchupsrocket":
                            case "launchups_rocket":
                                currentItem.LaunchUpsRocket = value;
                                break;
                            case "launchupsmarker3":
                            case "launchups_marker3":
                                currentItem.LaunchUpsMarker3 = value;
                                break;
                            case "soundprofile":
                                currentItem.SoundProfile = value;
                                break;
                            case "soundprofilefemale":
                                currentItem.SoundProfileFemale = value;
                                break;
                            case "sounddeath":
                                currentItem.SoundDeath = value;
                                break;
                            case "soundloop":
                                currentItem.SoundLoops.Add(value);
                                break;
                            case "soundloop_1":
                                currentItem.SoundLoop1 = value;
                                break;
                            case "soundloop_2":
                                currentItem.SoundLoop2 = value;
                                break;
                            case "soundloop_3":
                                currentItem.SoundLoop3 = value;
                                break;
                            case "soundloop_4":
                                currentItem.SoundLoop4 = value;
                                break;
                            case "numdoors":
                                if (byte.TryParse(value, out byte numDoors))
                                {
                                    currentItem.NumDoors = numDoors;
                                }
                                break;
                            case "firstdoor":
                                if (byte.TryParse(value, out byte firstDoor))
                                {
                                    currentItem.FirstDoor = firstDoor;
                                }
                                break;
                            case "doortype":
                                if (uint.TryParse(value, out uint doorType))
                                {
                                    currentItem.DoorType = doorType;
                                }
                                break;
                            case "doordir":
                                if (uint.TryParse(value, out uint doorDir))
                                {
                                    currentItem.DoorDir = doorDir;
                                }
                                break;
                            case "openrate":
                                if (int.TryParse(value, out int openRate))
                                {
                                    currentItem.OpenRate = openRate;
                                }
                                break;
                            case "maxangle":
                                if (int.TryParse(value, out int maxAngle))
                                {
                                    currentItem.MaxAngle = maxAngle;
                                }
                                break;
                            case "dooropensoundid":
                                currentItem.DoorOpenSoundId = value;
                                break;
                            case "doorclosesoundid":
                                currentItem.DoorCloseSoundId = value;
                                break;
                            case "particlefx":
                                currentItem.ParticleFx = value;
                                break;
                            case "particlefxslot":
                                currentItem.ParticleFxSlot = value;
                                break;
                            case "particlefxs":
                                currentItem.ParticleFxS = value;
                                break;
                            case "particlefxsslot":
                                currentItem.ParticleFxSSlot = value;
                                break;
                            case "particlefxw1":
                                currentItem.ParticleFxW1 = value;
                                break;
                            case "particlefxw1slot":
                                currentItem.ParticleFxW1Slot = value;
                                break;
                            case "particlefxw2":
                                currentItem.ParticleFxW2 = value;
                                break;
                            case "particlefxw2slot":
                                currentItem.ParticleFxW2Slot = value;
                                break;
                            case "particlefxw3":
                                currentItem.ParticleFxW3 = value;
                                break;
                            case "particlefxw3slot":
                                currentItem.ParticleFxW3Slot = value;
                                break;
                            case "particlefxw4":
                                currentItem.ParticleFxW4 = value;
                                break;
                            case "particlefxw4slot":
                                currentItem.ParticleFxW4Slot = value;
                                break;
                            case "particledeath":
                                currentItem.ParticleDeath = value;
                                break;
                            case "particleh2odeath":
                                currentItem.ParticleH2ODeath = value;
                                break;
                            case "particlefire":
                                currentItem.ParticleFire = value;
                                break;
                            case "particleother":
                                currentItem.ParticleOther = value;
                                break;
                            case "particlefinale":
                                currentItem.ParticleFinale = value;
                                break;
                            case "particlespawn":
                                currentItem.ParticleSpawn = value;
                                break;
                            case "dawnshot":
                                currentItem.DawnShot = value;
                                break;
                            case "dawnshotmintime":
                                if (int.TryParse(value, out int dawnMin))
                                {
                                    currentItem.DawnShotMinTime = dawnMin;
                                }
                                break;
                            case "dawnshotmaxtime":
                                if (int.TryParse(value, out int dawnMax))
                                {
                                    currentItem.DawnShotMaxTime = dawnMax;
                                }
                                break;
                            case "dayshot":
                                currentItem.DayShot = value;
                                break;
                            case "dayshotmintime":
                                if (int.TryParse(value, out int dayMin))
                                {
                                    currentItem.DayShotMinTime = dayMin;
                                }
                                break;
                            case "dayshotmaxtime":
                                if (int.TryParse(value, out int dayMax))
                                {
                                    currentItem.DayShotMaxTime = dayMax;
                                }
                                break;
                            case "duskshot":
                                currentItem.DuskShot = value;
                                break;
                            case "duskshotmintime":
                                if (int.TryParse(value, out int duskMin))
                                {
                                    currentItem.DuskShotMinTime = duskMin;
                                }
                                break;
                            case "duskshotmaxtime":
                                if (int.TryParse(value, out int duskMax))
                                {
                                    currentItem.DuskShotMaxTime = duskMax;
                                }
                                break;
                            case "nightshot":
                                currentItem.NightShot = value;
                                break;
                            case "nightshotmintime":
                                if (int.TryParse(value, out int nightMin))
                                {
                                    currentItem.NightShotMinTime = nightMin;
                                }
                                break;
                            case "nightshotmaxtime":
                                if (int.TryParse(value, out int nightMax))
                                {
                                    currentItem.NightShotMaxTime = nightMax;
                                }
                                break;
                            case "attrib:":
                                string[] attributeValues = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                                foreach (string attributeName in attributeValues)
                                {
                                    // Handle special cases for "good" and "evil"
                                    if (attributeName.Equals("good", StringComparison.OrdinalIgnoreCase))
                                    {
                                        currentItem.Good = true;
                                        if (currentItem.Evil)
                                        {
                                            throw new InvalidOperationException($"Item '{currentItem.Name}' cannot be both Good and Evil");
                                        }
                                    }
                                    else if (attributeName.Equals("evil", StringComparison.OrdinalIgnoreCase))
                                    {
                                        currentItem.Evil = true;
                                        if (currentItem.Good)
                                        {
                                            throw new InvalidOperationException($"Item '{currentItem.Name}' cannot be both Good and Evil");
                                        }
                                    }
                                    else if (Enum.TryParse<EntityAttributes>(attributeName, true, out var attributeFlag))
                                    {
                                        currentItem.Attributes |= attributeFlag;
                                    }
                                    else if (Enum.TryParse<EntityAttributes2>(attributeName, true, out var attribute2Flag))
                                    {
                                        currentItem.Attributes2 |= attribute2Flag;
                                    }
                                    else
                                    {
                                        GD.Print($"Unknown attribute '{attributeName}' in line: {line}");
                                    }
                                }
                                break;
                            case "primary_weapon":
                                currentItem.PrimaryWeapon = value;
                                break;
                            case "addeweap":
                                var addeweapParts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (addeweapParts.Length >= 2)
                                {
                                    var weapon = new EmplacedWeapon
                                    {
                                        Name = addeweapParts[0]
                                    };

                                    if (int.TryParse(addeweapParts[1], out int weaponId))
                                    {
                                        weapon.WeaponId = weaponId;
                                    }

                                    if (addeweapParts.Length >= 5)
                                    {
                                        float[] position = new float[3];
                                        if (float.TryParse(addeweapParts[2], out float x) &&
                                            float.TryParse(addeweapParts[3], out float y) &&
                                            float.TryParse(addeweapParts[4], out float z))
                                        {
                                            position[0] = x;
                                            position[1] = y;
                                            position[2] = z;
                                            weapon.Position = position;
                                        }
                                    }

                                    currentItem.AddEWeap = weapon;
                                }
                                break;
                            case "addeweapc":
                                var addeweapcParts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (addeweapcParts.Length >= 2)
                                {
                                    var weapon = new EmplacedWeapon
                                    {
                                        Name = addeweapcParts[0]
                                    };

                                    if (int.TryParse(addeweapcParts[1], out int weaponId))
                                    {
                                        weapon.WeaponId = weaponId;
                                    }

                                    if (addeweapcParts.Length >= 5)
                                    {
                                        float[] position = new float[3];
                                        if (float.TryParse(addeweapcParts[2], out float x) &&
                                            float.TryParse(addeweapcParts[3], out float y) &&
                                            float.TryParse(addeweapcParts[4], out float z))
                                        {
                                            position[0] = x;
                                            position[1] = y;
                                            position[2] = z;
                                            weapon.Position = position;
                                        }
                                    }

                                    currentItem.AddEWeapC = weapon;
                                }
                                break;
                            case "addeweapg":
                                var addeweapgParts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (addeweapgParts.Length >= 2)
                                {
                                    var weapon = new EmplacedWeapon
                                    {
                                        Name = addeweapgParts[0]
                                    };

                                    if (int.TryParse(addeweapgParts[1], out int weaponId))
                                    {
                                        weapon.WeaponId = weaponId;
                                    }

                                    if (addeweapgParts.Length >= 5)
                                    {
                                        float[] position = new float[3];
                                        if (float.TryParse(addeweapgParts[2], out float x) &&
                                            float.TryParse(addeweapgParts[3], out float y) &&
                                            float.TryParse(addeweapgParts[4], out float z))
                                        {
                                            position[0] = x;
                                            position[1] = y;
                                            position[2] = z;
                                            weapon.Position = position;
                                        }
                                    }

                                    currentItem.AddEWeapG = weapon;
                                }
                                break;
                            default:
                                GD.Print("Unknown item property " + key);
                                break;
                        }
                    }
                }
            }

            GD.Print($"ItemDefParser.Parse completed. Found {items.Count} items.");

            return items;
        }
        private string ExtractQuotedString(string text)
        {
            var match = Regex.Match(text, "\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : text.Trim();
        }
    }
}