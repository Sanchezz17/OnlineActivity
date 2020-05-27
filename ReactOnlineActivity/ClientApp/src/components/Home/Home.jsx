import React, { Component } from 'react';
import styles from './home.module.css';

export class Home extends Component {
    static displayName = Home.name;

    render() {
        return (
            <div>
                <h1>Игра «Онлайн-Активити»</h1>
                <p>Это игра, смысл которой объяснить загаданное слово при помощи рисования каких-то ассоциаций</p>
                <ul>
                    <li>
                        Вы можете <strong>начать игру</strong> в случайной комнате
                    </li>
                    <li>
                        <strong>Создайте комнату</strong> и настройте её под себя для игры с друзьями!
                    </li>
                </ul>
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
