create table book (
    id integer primary key asc not null,
    title string not null,
    author string,
    main_topic string,
    filepath string,
    modified string not null
);

create table reading_log (
    id integer primary key asc not null,
    id_book integer not null,
    initial_page integer not null,
    final_page integer not null,
    read string not null,
    modified string not null,
    next_topic string,
    FOREIGN KEY(id_book) REFERENCES book(id)
);

create view if not exists book_by_last_reading_log as
select distinct a.* 
    from book a 
    left join reading_log b on a.id = b.id_book 
    group by a.id, b.read 
    order by b.read desc;
