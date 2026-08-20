create table if not exists book (
    id integer primary key asc not null,
    title string not null,
    author string,
    main_topic string,
    filepath string,
    modified string not null
);

create table if not exists reading_log (
    id integer primary key asc not null,
    id_book integer not null,
    initial_page integer not null,
    final_page integer not null,
    read string not null,
    modified string not null,
    next_topic string,
    FOREIGN KEY(id_book) REFERENCES book(id)
);

create table if not exists hook (
    id integer primary key asc not null,
    name string not null,
    command string not null
);
