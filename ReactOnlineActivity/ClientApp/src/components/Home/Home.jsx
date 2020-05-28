import React, { Component } from 'react';
import styles from './home.module.css';

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <div>
                <h1>«Онлайн-Активити»</h1>
                <p>Это игра, смысл которой объяснить загаданное слово при помощи рисования каких-то ассоциаций</p>
                <h2>Правила игры</h2>
                <article>
                    <p>
                        Игра делится на несколько раундов.
                        В начале каждого раунда случайным образом выбирается ведущий,
                        которому нужно объяснить загаданное слово при помощи рисования каких-то ассоциаций
                        Задача остальных игроков отгадать загаданное слово.
                    </p>
                    <p>
                        Объясняющий имеет возможность отмечать идеи участников в чате,
                        тем самым показывая, что это правильный или неправильный ход мысли.
                    </p>
                    <p>
                        Раунд заканчивается, когда все участники отгадали слово или у ведущего закончилось время на объяснение.
                        Так же ведущий может сдаться, тем самым закончив раунд.
                        Игра длится до того момента, пока никто из участников не набирет определенное количество очков.
                    </p>
                </article>
                <ul>
                    <li>
                        Вы можете <strong>начать игру</strong> в случайной комнате
                    </li>
                    <li>
                        <strong>Создайте комнату</strong> и настройте её под себя для игры с друзьями!
                    </li>
                </ul>
                <h2 className={styles.wish}>
                    Удачной игры!
                </h2>
                <div className={styles.links}>
                    <a className={`btn btn-success btn-lg ${styles.link}`} href="/api/play">
                        Начать игру
                    </a>
                    <a className={`btn btn-warning btn-lg ${styles.link}`} href="/create">
                        Создать комнату
                    </a>
                </div>
            </div>
        );
    }
}
