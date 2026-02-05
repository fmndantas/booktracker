create table reading_log (
    id integer primary key asc,
    id_book integer not null,
    timestamp integer not null,
    FOREIGN KEY(id_book) REFERENCES book(id)
);
