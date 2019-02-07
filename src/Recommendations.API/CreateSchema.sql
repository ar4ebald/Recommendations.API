create schema if not exists rdb;

create type rdb.sex as enum ('Male', 'Female');

create table rdb.user (
  id serial primary key,

  age integer not null check (age >= 0),
  sex rdb.sex not null
);

create table rdb.category (
  id serial primary key,

  name text unique not null check (name <> ''),
  parent_id integer references rdb.category(id)
);

create table rdb.product (
  id serial primary key,

  name text unique not null check (name <> ''),
  category_id integer not null references rdb.category(id)
);

create table rdb.order (
  id serial primary key,
  product_id integer not null references rdb.product(id),
  user_id integer not null references rdb.user(id),
  day integer not null check (day >= 0)
);

create or replace function rdb.add_user(p_id integer, p_age integer, p_sex rdb.sex)
  returns void
as $$
  insert into rdb.user (id, age, sex) values (p_id, p_age, p_sex);
$$ language sql;

create or replace function rdb.add_category(p_name text, p_parent_name text)
  returns integer
as $$
declare
  v_parent_id integer;
  v_id integer;
begin
  select id into v_parent_id
  from rdb.category
  where name = p_parent_name;

  if not found then
    insert into rdb.category(name)
    values(p_parent_name)
    returning id into v_parent_id;
  end if;

  insert into rdb.category (name, parent_id)
  values (p_name, v_parent_id)
  on conflict (name) do update
    set parent_id = EXCLUDED.parent_id
  returning id into v_id;

  return v_id;
end;
$$ language plpgsql;

create or replace function rdb.add_product(p_id integer, p_name text, p_category_id integer)
  returns void as $$
  insert into rdb.product(id, name, category_id)
  values (p_id, p_name, p_category_id);
$$ language sql;

create or replace function rdb.add_order(p_order_id integer, p_product_id integer, p_user_id integer, p_day integer)
  returns void as $$
  insert into rdb.order (id, product_id, user_id, day)
  values (p_order_id, p_product_id, p_user_id, p_day);
$$ language sql;