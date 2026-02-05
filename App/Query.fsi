module App.Query

val getBooks: connectionString: string -> ReadDomain.Book list
