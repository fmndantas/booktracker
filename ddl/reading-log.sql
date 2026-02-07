create table reading_log (
    id integer primary key asc not null,
    id_book integer not null,
    initial_page integer not null,
    final_page integer not null,
    timestamp integer not null,
    next_topic string,
    FOREIGN KEY(id_book) REFERENCES book(id)
);
