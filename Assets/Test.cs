using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [Serializable]
    public struct item 
    {
        public GameObject hhh;
        public string aaa;
    }

    public List<item> test_Items;
}
