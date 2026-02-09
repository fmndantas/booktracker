module App.SqliteExtensions

open System

type DateTime with
  member i.ToSqlite: string = i.ToString "o"

type String with
  member i.FromSqlite: DateTime = DateTime.Parse(i).ToUniversalTime()
