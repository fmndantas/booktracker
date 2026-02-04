create table book (
    id integer primary key asc,
    title string not null,
    author string not null,
    genre string not null
);

create table reading_log (
    id integer primary key asc,
    id_book integer not null,
    foreign key(id_book) references book(id)
);
