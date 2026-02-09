create table book (
    id integer primary key asc not null,
    title string not null,
    author string,
    main_topic string,
    filepath string,
    modified string not null
);
