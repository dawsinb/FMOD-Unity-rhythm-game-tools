using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Decoder {
  public static Dictionary<string, string> GetArgs(string encodedString) {
    // match parameters
    var matches = Regex.Matches(encodedString, @"{(?<name>[\w\d]+)=(?<value>[\w\d]+)}");

    // create dict to hold the locations
    Dictionary<string, string> args = new Dictionary<string, string>();

    // loop through all matches
    for (int i = 0; i < matches.Count; i++) {
      // get groups of current match
      GroupCollection groups = matches[i].Groups;
      // add match to the array
      args.Add(groups["name"].Value, groups["value"].Value);
    }

    return args;
  }
}
