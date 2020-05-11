import React, {Component} from 'react';
import styles from './home.module.css';

export class Home extends Component {
    static displayName = Home.name;

    render() {
        const { user } = this.props;
        
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
                <div className={styles.buttons}>
                    <a className={`btn btn-success btn-lg ${styles.button}`} href={`/api/play?userName=${user.name}`}>
                        Начать игру
                    </a>
                    <a className={`btn btn-warning btn-lg ${styles.button}`} href="#">
                        Создать комнату
                    </a>
                </div>
            </div>
        );
    }
}
