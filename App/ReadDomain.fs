module App.ReadDomain

type Book = { Id: int64; Title: string }

let createBook (id: int64) (title: string) : Book = { Id = id; Title = title }
