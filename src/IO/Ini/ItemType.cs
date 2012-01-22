﻿// * ************************************************************
// * * START:                                       itemtype.cs *
// * ************************************************************

// * ************************************************************
// *                      INFORMATIONS
// * ************************************************************
// * ItemType class for the library.
// * itemtype.cs
// * 
// * --
// *
// * Feel free to use this class in your projects, but don't
// * remove the header to keep the paternity of the class.
// * 
// * ************************************************************
// *                      CREDITS
// * ************************************************************
// * Originally created by CptSky (January 22th, 2012)
// * Copyright (C) 2012 CptSky
// *
// * ************************************************************

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CO2_CORE_DLL.IO
{
    /// <summary>
    /// ItemType
    /// </summary>
    public unsafe class ItemType
    {
        public const Int32 MAX_NAMESIZE = 0x10;
        public const Int32 MAX_DESCSIZE = 128;

        //Item monopoly bit field
        public const Int32 ITEM_MONOPOLY_MASK				= 0x01;
        public const Int32 ITEM_STORAGE_MASK				= 0x02;
        public const Int32 ITEM_DROP_HINT_MASK				= 0x04;
        public const Int32 ITEM_SELL_HINT_MASK				= 0x08;
        public const Int32 ITEM_NEVER_DROP_WHEN_DEAD_MASK	= 0x10;
        public const Int32 ITEM_SELL_DISABLE_MASK			= 0x20;

        //Item status bit field
        public const Int32 ITEM_STATUS_NONE = 0;
        public const Int32 ITEM_STATUS_NOT_IDENT = 1;
        public const Int32 ITEM_STATUS_CANNOT_REPAIR = 2;
        public const Int32 ITEM_STATUS_NEVER_DAMAGE = 4;
        public const Int32 ITEM_STATUS_MAGIC_ADD = 8;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            public Int32 ID;
            public fixed Byte Name[MAX_NAMESIZE];
            public Byte RequiredProfession;
            public Byte RequiredWeaponSkill;
            public Byte RequiredLevel;
            public Byte RequiredSex;
            public UInt16 RequiredForce;
            public UInt16 RequiredSpeed;
            public UInt16 RequiredHealth;
            public UInt16 RequiredSoul;
            public Byte Monopoly;
            public UInt16 Weight;
            public UInt32 Price;
            public Int32 Action; //cq_action
            public UInt16 MaxAttack;
            public UInt16 MinAttack;
            public Int16 Defense;
            public Int16 Dexterity;
            public Int16 Dodge;
            public Int16 Life;
            public Int16 Mana;
            public UInt16 Amount;
            public UInt16 AmountLimit; // x / 100
            public Byte Status;
            public Byte Gem1;
            public Byte Gem2;
            public Byte Magic1;
            public Byte Magic2;
            public Byte Magic3;
            public UInt16 MagicAttack;
            public UInt16 MagicDefence;
            public UInt16 Range;
            public UInt16 AttackSpeed;
            public Byte FrayMode; //Good name?
            public Byte RepairMode; //Good name?
            public Byte TypeMask; //Good name?
            public UInt32 ConquerPoints; //Good name?
            public fixed Byte Type[MAX_NAMESIZE]; //Good name?
            public fixed Byte Desc[MAX_DESCSIZE];
        };

        private Dictionary<Int32, IntPtr> Entries = null;

        /// <summary>
        /// Create a new ItemType instance to handle the TQ's ItemType file.
        /// </summary>
        public ItemType()
        {
            this.Entries = new Dictionary<Int32, IntPtr>();
        }

        ~ItemType()
        {
            Clear();
        }

        /// <summary>
        /// Reset the dictionary and free all the used memory.
        /// </summary>
        public void Clear()
        {
            if (Entries != null)
            {
                lock (Entries)
                {
                    foreach (IntPtr Ptr in Entries.Values)
                        Kernel.free((Entry*)Ptr);
                }
                Entries.Clear();
            }
        }

        /// <summary>
        /// Load the specified itemtype file (in custom binary format) into the dictionary.
        /// </summary>
        public void LoadFromDat(String Path)
        {
            Clear();

            lock (Entries)
            {
                using (FileStream Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Byte[] Buffer = new Byte[Kernel.MAX_BUFFER_SIZE];
                    Entries = new Dictionary<Int32, IntPtr>();

                    while (Stream.Read(Buffer, 0, sizeof(Entry)) == sizeof(Entry))
                    {
                        Entry* pEntry = (Entry*)Kernel.malloc(sizeof(Entry));
                        Kernel.memcpy((Byte*)pEntry, Buffer, sizeof(Entry));

                        if (!Entries.ContainsKey(pEntry->ID))
                            Entries.Add(pEntry->ID, (IntPtr)pEntry);
                    }
                }
            }
        }

        /// <summary>
        /// Load the specified itemtype file (in plain format) into the dictionary.
        /// </summary>
        public void LoadFromTxt(String Path)
        {
            Clear();

            lock (Entries)
            {
                using (StreamReader Stream = new StreamReader(Path, Encoding.GetEncoding("Windows-1252")))
                {
                    Entries = new Dictionary<Int32, IntPtr>();

                    String Line = null;
                    Int32 LineC = 0;
                    while ((Line = Stream.ReadLine()) != null)
                    {
                        if (LineC == 0) //Amount = X
                        {
                            LineC++;
                            continue;
                        }
                        LineC++;

                        String[] Parts = Line.Split(' ');
                        Entry* pEntry = (Entry*)Kernel.calloc(sizeof(Entry));

                        try
                        {
                            pEntry->ID = Int32.Parse(Parts[0]);
                            Kernel.memcpy(pEntry->Name, Parts[1].ToPointer(), Math.Min(MAX_NAMESIZE - 1, Parts[1].Length));
                            pEntry->RequiredProfession = Byte.Parse(Parts[2]);
                            pEntry->RequiredWeaponSkill = Byte.Parse(Parts[3]);
                            pEntry->RequiredLevel = Byte.Parse(Parts[4]);
                            pEntry->RequiredSex = Byte.Parse(Parts[5]);
                            pEntry->RequiredForce = UInt16.Parse(Parts[6]);
                            pEntry->RequiredSpeed = UInt16.Parse(Parts[7]);
                            pEntry->RequiredHealth = UInt16.Parse(Parts[8]);
                            pEntry->RequiredSoul = UInt16.Parse(Parts[9]);
                            pEntry->Monopoly = Byte.Parse(Parts[10]);
                            pEntry->Weight = UInt16.Parse(Parts[11]);
                            pEntry->Price = UInt32.Parse(Parts[12]);
                            pEntry->Action = Int32.Parse(Parts[13]);
                            pEntry->MaxAttack = UInt16.Parse(Parts[14]);
                            pEntry->MinAttack = UInt16.Parse(Parts[15]);
                            pEntry->Defense = Int16.Parse(Parts[16]);
                            pEntry->Dexterity = Int16.Parse(Parts[17]);
                            pEntry->Dodge = Int16.Parse(Parts[18]);
                            pEntry->Life = Int16.Parse(Parts[19]);
                            pEntry->Mana = Int16.Parse(Parts[20]);
                            pEntry->Amount = UInt16.Parse(Parts[21]);
                            pEntry->AmountLimit = UInt16.Parse(Parts[22]);
                            pEntry->Status = Byte.Parse(Parts[23]);
                            pEntry->Gem1 = Byte.Parse(Parts[24]);
                            pEntry->Gem2 = Byte.Parse(Parts[25]);
                            pEntry->Magic1 = Byte.Parse(Parts[26]);
                            pEntry->Magic2 = Byte.Parse(Parts[27]);
                            pEntry->Magic3 = Byte.Parse(Parts[28]);
                            pEntry->MagicAttack = UInt16.Parse(Parts[29]);
                            pEntry->MagicDefence = UInt16.Parse(Parts[30]);
                            pEntry->Range = UInt16.Parse(Parts[31]);
                            pEntry->AttackSpeed = UInt16.Parse(Parts[32]);
                            pEntry->FrayMode = Byte.Parse(Parts[33]);
                            pEntry->RepairMode = Byte.Parse(Parts[34]);
                            pEntry->TypeMask = Byte.Parse(Parts[35]);
                            pEntry->ConquerPoints = UInt32.Parse(Parts[36]);
                            Kernel.memcpy(pEntry->Type, Parts[37].ToPointer(), Math.Min(MAX_NAMESIZE - 1, Parts[37].Length));
                            Kernel.memcpy(pEntry->Desc, Parts[38].ToPointer(), Math.Min(MAX_DESCSIZE - 1, Parts[38].Length));

                            if (!Entries.ContainsKey(pEntry->ID))
                                Entries.Add(pEntry->ID, (IntPtr)pEntry);
                        }
                        catch (Exception Exc)
                        {
                            Console.WriteLine("Error at line {0}.\n{1}", LineC, Exc);
                            Kernel.free(pEntry);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save all the dictionary to the specified itemtype file (in custom binary format).
        /// </summary>
        public void SaveToDat(String Path)
        {
            using (FileStream Stream = new FileStream(Path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                Byte[] Buffer = new Byte[Kernel.MAX_BUFFER_SIZE];
                IntPtr[] Pointers = new IntPtr[0];

                lock (Entries)
                {
                    Pointers = new IntPtr[Entries.Count];
                    Entries.Values.CopyTo(Pointers, 0);
                }

                for (Int32 i = 0; i < Pointers.Length; i++)
                {
                    Kernel.memcpy(Buffer, (Entry*)Pointers[i], sizeof(Entry));
                    Stream.Write(Buffer, 0, sizeof(Entry));
                }
            }
        }

        /// <summary>
        /// Save all the dictionary to the specified itemtype file (in plain format).
        /// </summary>
        public void SaveToTxt(String Path)
        {
            using (StreamWriter Stream = new StreamWriter(Path, false, Encoding.GetEncoding("Windows-1252")))
            {
                IntPtr[] Pointers = new IntPtr[0];

                lock (Entries)
                {
                    Pointers = new IntPtr[Entries.Count];
                    Entries.Values.CopyTo(Pointers, 0);
                }

                Stream.WriteLine("Amount={0}", Pointers.Length);
                for (Int32 i = 0; i < Pointers.Length; i++)
                {
                    Entry* pEntry = (Entry*)Pointers[i];

                    StringBuilder Builder = new StringBuilder(Kernel.MAX_BUFFER_SIZE);
                    Builder.Append(pEntry->ID + " ");
                    Builder.Append(Kernel.cstring(pEntry->Name, MAX_NAMESIZE) + " ");
                    Builder.Append(pEntry->RequiredProfession + " ");
                    Builder.Append(pEntry->RequiredWeaponSkill + " ");
                    Builder.Append(pEntry->RequiredLevel + " ");
                    Builder.Append(pEntry->RequiredSex + " ");
                    Builder.Append(pEntry->RequiredForce + " ");
                    Builder.Append(pEntry->RequiredSpeed + " ");
                    Builder.Append(pEntry->RequiredHealth + " ");
                    Builder.Append(pEntry->RequiredSoul + " ");
                    Builder.Append(pEntry->Monopoly + " ");
                    Builder.Append(pEntry->Weight + " ");
                    Builder.Append(pEntry->Price + " ");
                    Builder.Append(pEntry->Action + " ");
                    Builder.Append(pEntry->MaxAttack + " ");
                    Builder.Append(pEntry->MinAttack + " ");
                    Builder.Append(pEntry->Defense + " ");
                    Builder.Append(pEntry->Dexterity + " ");
                    Builder.Append(pEntry->Dodge + " ");
                    Builder.Append(pEntry->Life + " ");
                    Builder.Append(pEntry->Mana + " ");
                    Builder.Append(pEntry->Amount + " ");
                    Builder.Append(pEntry->AmountLimit + " ");
                    Builder.Append(pEntry->Status + " ");
                    Builder.Append(pEntry->Gem1 + " ");
                    Builder.Append(pEntry->Gem2 + " ");
                    Builder.Append(pEntry->Magic1 + " ");
                    Builder.Append(pEntry->Magic2 + " ");
                    Builder.Append(pEntry->Magic3 + " ");
                    Builder.Append(pEntry->MagicAttack + " ");
                    Builder.Append(pEntry->MagicDefence + " ");
                    Builder.Append(pEntry->Range + " ");
                    Builder.Append(pEntry->AttackSpeed + " ");
                    Builder.Append(pEntry->FrayMode + " ");
                    Builder.Append(pEntry->RepairMode + " ");
                    Builder.Append(pEntry->TypeMask + " ");
                    Builder.Append(pEntry->ConquerPoints + " ");
                    Builder.Append(Kernel.cstring(pEntry->Type, MAX_NAMESIZE) + " ");
                    Builder.Append(Kernel.cstring(pEntry->Desc, MAX_DESCSIZE));
                    Stream.WriteLine(Builder.ToString());
                }
            }
        }

        /// <summary>
        /// Get the information of the specified type for the specified item.
        /// To get a valid string for some fields, use Kernel.cstring function.
        /// </summary>
        public Boolean GetItemTypeInfo(Int32 ID, ref Entry Entry)
        {
            Entry = new Entry();
            lock (Entries)
            {
                if (Entries.ContainsKey(ID))
                {
                    fixed (Entry* pEntry = &Entry)
                        Kernel.memcpy(pEntry, (Entry*)Entries[ID], sizeof(Entry));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add the item's information in the dictionary.
        /// It can be used to create an editor or an temp item.
        /// </summary>
        public Boolean AddItemTypeInfo(Entry Entry)
        {
            lock (Entries)
            {
                if (!Entries.ContainsKey(Entry.ID))
                {
                    Entry* pEntry = (Entry*)Kernel.calloc(sizeof(Entry));
                    Kernel.memcpy(pEntry, &Entry, sizeof(Entry));

                    Entries.Add(Entry.ID, (IntPtr)pEntry);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Delete the item's information in the dictionary.
        /// </summary>
        public Boolean DelItemTypeInfo(Int32 ID)
        {
            lock (Entries)
            {
                if (Entries.ContainsKey(ID))
                {
                    Entries.Remove(ID);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Update the item's information in the dictionary.
        /// </summary>
        public Boolean UpdItemTypeInfo(Int32 ID, Entry Entry)
        {
            lock (Entries)
            {
                if (Entries.ContainsKey(ID))
                {
                    Kernel.memcpy((Entry*)Entries[ID], &Entry, sizeof(Entry));
                    return true;
                }
            }
            return false;
        }
    }
}

// * ************************************************************
// * * END:                                         itemtype.cs *
// * ************************************************************