create view if not exists book_by_last_reading_log as
select distinct a.* 
    from book a 
    join reading_log b on a.id = b.id_book 
    group by a.id, b.read 
    order by b.read desc;
