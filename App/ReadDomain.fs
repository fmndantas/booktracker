module App.ReadDomain

type BookId = BookId of int64

type Book = { Id: BookId; Title: string }

let createBook (id: BookId) (title: string) : Book = { Id = id; Title = title }
