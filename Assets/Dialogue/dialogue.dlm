STATE s_questItem DEFAULT (bool)false
STATE s_hasItem DEFAULT (bool)false

npc_jar:
    IF !s_questItem {
        c_jar \ Hi, could you get me the item? #right
        CHOICE {
            c_player * Yes #front3
        accept_quest:
            c_jar \ Okay, thanks. #front3
            RUN s_questItem = true
            HALT

            c_player * No #front1
            c_jar \ Please?
            CHOICE {
                c_player * Fine #front3
                GOTO accept_quest

                c_player * Still no #front1
                c_jar \ Shame on you. #front3
                HALT
            }

            c_player * Maybe later #front2
            c_jar \ I'll wait. #left
            HALT
        }
    } ELSE {
        IF s_hasItem {
            c_jar \ Thank you for the item. #right
            HALT
        } ELSE {
            c_jar \ I'm waiting for the item. #left
            HALT
        }
    }
