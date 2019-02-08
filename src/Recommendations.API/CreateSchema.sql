create schema if not exists rdb;

create type rdb.sex as enum ('male', 'female');

create type rdb.user_type as (
  id integer, age integer, sex rdb.sex
);

create type rdb.order_type as (
  id integer, user_id integer, day integer
);

create type rdb.order_entry_type as (
  order_id integer, product_id integer
);

create type rdb.operator_type as (
  id integer, login text, settings jsonb
);

create table rdb.user (
  id  serial primary key,

  age integer not null check (age >= 0),
  sex rdb.sex not null
);

create table rdb.category (
  id        serial primary key,

  name      text not null check (name <> ''),
  parent_id integer references rdb.category (id)
);

create table rdb.product (
  id          serial primary key,

  name        text unique not null check (name <> ''),
  category_id integer     not null references rdb.category (id)
);

create table rdb.order (
  id         serial primary key,
  user_id    integer not null references rdb.user (id),
  day        integer not null check (day >= 0)
);

create table rdb.order_entry (
  order_id   integer not null references rdb.order (id),
  product_id integer not null references rdb.product (id),

  primary key (order_id, product_id)
);

create table rdb.operator (
  id serial primary key,

  login text not null unique check(login <> ''),
  password text not null,

  settings jsonb
);

create or replace function rdb.add_user(p_users rdb.user_type [])
  returns void
as $$
  insert into rdb.user(id, age, sex)
  select id, age, sex
  from unnest(p_users) inserting(id, age, sex)
$$
language sql;

create or replace function rdb.add_category(p_id integer[], p_name text[], p_parent_id integer[])
  returns void
as $$
  insert into rdb.category(id, name, parent_id)
  select id, name, nullif(parent_id, 0)
  from unnest(p_id, p_name, p_parent_id) inserting(id, name, parent_id)
$$ language sql;

create or replace function rdb.add_product(p_id integer[], p_name text[], p_category_id integer[])
  returns void
as $$
  insert into rdb.product(id, name, category_id)
  select id, name, category_id
  from unnest(p_id, p_name, p_category_id) inserting(id, name, category_id)
$$
language sql;

create or replace function rdb.add_order(p_orders rdb.order_type[])
  returns void
as $$
  insert into rdb.order(id, user_id, day)
  select id, user_id, day
  from unnest(p_orders) inserting(id, user_id, day)
$$
language sql;

create or replace function rdb.add_order_entry(p_entries rdb.order_entry_type[])
  returns void
as $$
  insert into rdb.order_entry(order_id, product_id)
  select order_id, product_id
  from unnest(p_entries) inserting(order_id, product_id)
$$ language sql;

create or replace function rdb.get_operator(p_login text)
  returns table(operator rdb.operator_type, password text)
as $$
  select (id, login, settings)::rdb.operator_type, password
  from rdb.operator
  where login = p_login
$$ language sql;

create or replace function rdb.set_settings(p_operator_id integer, p_settings jsonb)
  returns void
as $$
  update rdb.operator
  set settings = p_settings
  where id = p_operator_id;
$$ language sql;